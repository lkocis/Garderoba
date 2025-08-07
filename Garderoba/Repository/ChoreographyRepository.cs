using Garderoba.Common;
using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.WebApi.ViewModel;
using Npgsql;

namespace Garderoba.Repository
{
    public class ChoreographyRepository : IChoreographyRepository
    {
        private const string _connectionString = "Host=localhost;Port=5433;Username=postgres;Password=PeLana2606;Database=Garderoba";

        public async Task<bool> CreateNewChoreographyAsync(Choreography choreography)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandTextChoreographyName = "SELECT * FROM \"Choreography\" WHERE \"Name\" = @Name;";
                var checkCommand = new NpgsqlCommand(commandTextChoreographyName, connection);
                checkCommand.Parameters.AddWithValue("@Name", choreography.Name);

                var exists = false;
                using (var reader = await checkCommand.ExecuteReaderAsync())
                {
                    exists = await reader.ReadAsync();
                }
                if (exists)
                {
                    Console.WriteLine("This choreography already exists.");
                    return false;
                }

                var commandText = @"
                INSERT INTO ""Choreography"" (
                    ""Name"",
                    ""Area"",
                    ""MenCostumeCount"",
                    ""WomenCostumeCount"",
                    ""DateCreated"",
                    ""DateUpdated"",
                    ""CreatedByUserId"")
                VALUES (
                    @Name,
                    @Area,
                    @MenCostumeCount,
                    @WomenCostumeCount,
                    @DateCreated,
                    @DateUpdated,
                    @CreatedByUserId
                );";

                using var insertCommand = new NpgsqlCommand(commandText, connection);

                insertCommand.Parameters.AddWithValue("@Name", choreography.Name ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Area", choreography.Area ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@MenCostumeCount", choreography.MenCostumeCount);
                insertCommand.Parameters.AddWithValue("@WomenCostumeCount", choreography.WomenCostumeCount);
                insertCommand.Parameters.AddWithValue("@DateCreated", choreography.DateCreated ?? DateTime.UtcNow);
                insertCommand.Parameters.AddWithValue("@DateUpdated", choreography.DateUpdated ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@CreatedByUserId", choreography.CreatedByUserId);

                int numberOfCommits = await insertCommand.ExecuteNonQueryAsync();

                Console.WriteLine(numberOfCommits > 0 ? "Choreography successfully created!" : "Choreography creation failed.");
                return numberOfCommits > 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error during creation: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteChoreographyByIdAsync(Guid id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandText = @"DELETE FROM ""Choreography"" WHERE ""Id"" = @Id;";
                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@Id", id);
                int rowsAffected = await command.ExecuteNonQueryAsync();

                Console.WriteLine(rowsAffected > 0 ? "Choreography successfully deleted!" : "Choreography deleting failed.");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during deleting: " + ex.Message);
                return false;
            }
        }

        public async Task<List<Choreography>> GetAllChoreographiesAsync()
        {
            var choreographies = new List<Choreography>();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandText = @"SELECT * FROM ""Choreography"";";
                using var command = new NpgsqlCommand(commandText, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var choreography = new Choreography
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        Name = reader["Name"] as string,
                        Area = reader["Area"] as string,
                        MenCostumeCount = reader.GetInt32(reader.GetOrdinal("MenCostumeCount")),
                        WomenCostumeCount = reader.GetInt32(reader.GetOrdinal("WomenCostumeCount")),
                        DateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                        DateUpdated = reader.IsDBNull(reader.GetOrdinal("DateUpdated"))
                            ? (DateTime?)null
                            : reader.GetDateTime(reader.GetOrdinal("DateUpdated")),
                        CreatedByUserId = reader.GetGuid(reader.GetOrdinal("CreatedByUserId"))
                    };

                    choreographies.Add(choreography);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fetching all choreographies: " + ex.Message);
            }

            return choreographies;
        }

        public async Task<bool> UpdateChoreographyByIdAsync(Guid id, UpdatedChoreographyFields updatedChoreography)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                Choreography existingChoreo = null;
                var getExistingChoreo = @"SELECT * FROM ""Choreography"" WHERE ""Id"" = @Id";

                using (var existingCommand = new NpgsqlCommand(getExistingChoreo, connection))
                {
                    existingCommand.Parameters.AddWithValue("@Id", id);

                    using var reader = await existingCommand.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        existingChoreo = new Choreography
                        {
                            Id = Guid.Parse(reader["Id"].ToString()),
                            Name = reader["Name"]?.ToString(),
                            Area = reader["Area"]?.ToString(),
                            MenCostumeCount = Convert.ToInt32(reader["MenCostumeCount"]),
                            WomenCostumeCount = Convert.ToInt32(reader["WomenCostumeCount"])
                        };
                    }
                    else
                    {
                        Console.WriteLine("Choreography not found.");
                        return false;
                    }
                }

                existingChoreo.Name = updatedChoreography.Name ?? existingChoreo.Name;
                existingChoreo.Area = updatedChoreography.Area ?? existingChoreo.Area;
                existingChoreo.MenCostumeCount = updatedChoreography.MenCostumeCount ?? existingChoreo.MenCostumeCount;
                existingChoreo.WomenCostumeCount = updatedChoreography.WomenCostumeCount ?? existingChoreo.WomenCostumeCount;

                using var updateCmd = new NpgsqlCommand(@"
                                                    UPDATE ""Choreography"" SET
                                                        ""Name"" = @Name,
                                                        ""Area"" = @Area,
                                                        ""MenCostumeCount"" = @MenCostumeCount,
                                                        ""WomenCostumeCount"" = @WomenCostumeCount,
                                                        ""DateUpdated"" = @DateUpdated
                                                    WHERE ""Id"" = @Id;", connection);

                updateCmd.Parameters.AddWithValue("@Name", existingChoreo.Name ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Area", existingChoreo.Area ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@MenCostumeCount", existingChoreo.MenCostumeCount);
                updateCmd.Parameters.AddWithValue("@WomenCostumeCount", existingChoreo.WomenCostumeCount);
                updateCmd.Parameters.AddWithValue("@DateUpdated", DateTime.UtcNow);
                updateCmd.Parameters.AddWithValue("@Id", id);

                int affectedRows = await updateCmd.ExecuteNonQueryAsync();

                if (affectedRows > 0)
                {
                    Console.WriteLine("Choreography successfully updated!");
                }

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating choreography: " + ex.Message);
                return false;
            }
        }

        public async Task<Choreography> GetChoreographyByIdAsync(Guid id)
        {
            Choreography choreography = null;
            var costumesDict = new Dictionary<Guid, Costume>();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var commandText = @"
                            SELECT 
                                c.""Id"" as ""ChoreographyId"", c.""Name"" as ""ChoreographyName"", c.""Area"", 
                                c.""MenCostumeCount"", c.""WomenCostumeCount"", c.""DateCreated"", c.""DateUpdated"",
                                c.""CreatedByUserId"",

                                co.""Id"" as ""CostumeId"", co.""Name"" as ""CostumeName"", co.""Area"" as ""CostumeArea"",
                                co.""Gender"", co.""Status"", co.""DateCreated"" as ""CostumeDateCreated"", co.""DateUpdated"" as ""CostumeDateUpdated"",
                                co.""CreatedByUserId"" as ""CostumeCreatedByUserId"",

                                cp.""Id"" as ""CostumePartId"", cp.""Region"", cp.""Name"" as ""CostumePartName"", cp.""PartNumber"", cp.""Status"" as ""CostumePartStatus"", cp.""DateCreated"" as ""CostumePartDateCreated""

                            FROM ""Choreography"" c
                            LEFT JOIN ""ChoreographyCostume"" cc ON c.""Id"" = cc.""ChoreographyId""
                            LEFT JOIN ""Costume"" co ON cc.""CostumeId"" = co.""Id""
                            LEFT JOIN ""CostumePart"" cp ON co.""Id"" = cp.""CostumeId""
                            WHERE c.""Id"" = @id;";

            using var command = new NpgsqlCommand(commandText, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (choreography == null)
                {
                    choreography = new Choreography
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("ChoreographyId")),
                        Name = reader["ChoreographyName"].ToString(),
                        Area = reader["Area"].ToString(),
                        MenCostumeCount = reader.GetInt32(reader.GetOrdinal("MenCostumeCount")),
                        WomenCostumeCount = reader.GetInt32(reader.GetOrdinal("WomenCostumeCount")),
                        DateCreated = reader.IsDBNull(reader.GetOrdinal("DateCreated")) ? null : reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                        DateUpdated = reader.IsDBNull(reader.GetOrdinal("DateUpdated")) ? null : reader.GetDateTime(reader.GetOrdinal("DateUpdated")),
                        CreatedByUserId = reader.GetGuid(reader.GetOrdinal("CreatedByUserId")),
                        ChoreographyCostumes = new List<ChoreographyCostume>()
                    };
                }

                if (!reader.IsDBNull(reader.GetOrdinal("CostumeId")))
                {
                    var costumeId = reader.GetGuid(reader.GetOrdinal("CostumeId"));
                    if (!costumesDict.TryGetValue(costumeId, out var costume))
                    {
                        costume = new Costume
                        {
                            Id = costumeId,
                            Name = reader["CostumeName"].ToString(),
                            Area = reader["CostumeArea"].ToString(),
                            Gender = (Gender)reader.GetInt32(reader.GetOrdinal("Gender")),
                            Status = (CostumeStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                            DateCreated = reader.IsDBNull(reader.GetOrdinal("CostumeDateCreated")) ? null : reader.GetDateTime(reader.GetOrdinal("CostumeDateCreated")),
                            DateUpdated = reader.IsDBNull(reader.GetOrdinal("CostumeDateUpdated")) ? null : reader.GetDateTime(reader.GetOrdinal("CostumeDateUpdated")),
                            CreatedByUserId = reader.GetGuid(reader.GetOrdinal("CostumeCreatedByUserId")),
                            Parts = new List<CostumePart>(),
                            ChoreographyCostumes = new List<ChoreographyCostume>()
                        };

                        costumesDict.Add(costumeId, costume);

                        choreography.ChoreographyCostumes.Add(new ChoreographyCostume
                        {
                            ChoreographyId = choreography.Id,
                            CostumeId = costume.Id,
                            Costume = costume,
                            Choreography = choreography
                        });
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("CostumePartId")))
                    {
                        var part = new CostumePart
                        {
                            Id = reader.GetGuid(reader.GetOrdinal("CostumePartId")),
                            CostumeId = costumeId,
                            Region = reader["Region"].ToString(),
                            Name = reader["CostumePartName"].ToString(),
                            PartNumber = reader.GetInt32(reader.GetOrdinal("PartNumber")),
                            Status = (CostumeStatus)reader.GetInt32(reader.GetOrdinal("CostumePartStatus")),
                            DateCreated = reader.IsDBNull(reader.GetOrdinal("CostumePartDateCreated")) ? null : reader.GetDateTime(reader.GetOrdinal("CostumePartDateCreated"))
                        };

                        costume.Parts.Add(part);
                    }
                }
            }

            return choreography;
        }
    }
}
