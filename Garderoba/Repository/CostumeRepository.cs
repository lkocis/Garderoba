using Garderoba.Common;
using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.WebApi.ViewModel;
using Npgsql;
using System.Transactions;
using System.Xml.Linq;

namespace Garderoba.Repository
{
    public class CostumeRepository : ICostumeRepository
    {
        private const string _connectionString = "Host=localhost;Port=5433;Username=postgres;Password=PeLana2606;Database=Garderoba";

        public async Task<bool> CreateNewCostumeAsync(Costume costume, Guid? choreographyId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var checkText = @"SELECT 1 FROM ""Costume"" WHERE ""Name"" = @Name;";
                using var checkCommand = new NpgsqlCommand(checkText, connection);
                checkCommand.Parameters.AddWithValue("@Name", costume.Name);

                var exists = false;
                using (var reader = await checkCommand.ExecuteReaderAsync())
                {
                    exists = await reader.ReadAsync();
                }

                if (exists)
                {
                    Console.WriteLine("This costume already exists.");
                    return false;
                }

                var now = DateTime.UtcNow;

                var insertCostumeText = @"INSERT INTO ""Costume"" (
                                ""Name"", 
                                ""Area"", 
                                ""Gender"", 
                                ""Status"",
                                ""NecessaryParts"",
                                ""DateCreated"", 
                                ""CreatedByUserId"")
                            VALUES (
                                @Name, 
                                @Area, 
                                @Gender, 
                                @Status, 
                                @NecessaryParts,
                                @DateCreated, 
                                @CreatedByUserId)
                            RETURNING ""Id"";";

                using var insertCostumeCmd = new NpgsqlCommand(insertCostumeText, connection);

                insertCostumeCmd.Parameters.AddWithValue("@Name", costume.Name ?? (object)DBNull.Value);
                insertCostumeCmd.Parameters.AddWithValue("@Area", costume.Area ?? (object)DBNull.Value);
                insertCostumeCmd.Parameters.AddWithValue("@Gender", (int)costume.Gender);
                insertCostumeCmd.Parameters.AddWithValue("@Status", (int)costume.Status);
                insertCostumeCmd.Parameters.AddWithValue("@NecessaryParts", costume.NecessaryParts ?? (object)DBNull.Value);
                insertCostumeCmd.Parameters.AddWithValue("@DateCreated", now);
                insertCostumeCmd.Parameters.AddWithValue("@CreatedByUserId", costume.CreatedByUserId);

                var costumeId = (Guid)await insertCostumeCmd.ExecuteScalarAsync();

                var insertChoreoCostumeText = @"INSERT INTO ""ChoreographyCostume"" (
                                        ""ChoreographyId"",
                                        ""CostumeId"")
                                    VALUES (
                                        @ChoreographyId,
                                        @CostumeId);";

                using var insertChoreoCostumeCmd = new NpgsqlCommand(insertChoreoCostumeText, connection);
                insertChoreoCostumeCmd.Parameters.AddWithValue("@ChoreographyId", choreographyId);
                insertChoreoCostumeCmd.Parameters.AddWithValue("@CostumeId", costumeId);

                await insertChoreoCostumeCmd.ExecuteNonQueryAsync();

                if (costume.Parts != null && costume.Parts.Count > 0)
                {
                    foreach (var part in costume.Parts)
                    {
                        var insertPartText = @"INSERT INTO ""CostumePart"" (
                                    ""CostumeId"", 
                                    ""Region"", 
                                    ""Name"", 
                                    ""PartNumber"", 
                                    ""Status"", 
                                    ""Gender"",
                                    ""DateCreated"")
                                VALUES (
                                    @CostumeId, 
                                    @Region, 
                                    @Name, 
                                    @PartNumber, 
                                    @Status, 
                                    @Gender,
                                    @DateCreated);";

                        using var insertPartCmd = new NpgsqlCommand(insertPartText, connection);
                        insertPartCmd.Parameters.AddWithValue("@CostumeId", costumeId);
                        insertPartCmd.Parameters.AddWithValue("@Region", part.Region ?? (object)DBNull.Value);
                        insertPartCmd.Parameters.AddWithValue("@Name", part.Name ?? (object)DBNull.Value);
                        insertPartCmd.Parameters.AddWithValue("@PartNumber", part.PartNumber);
                        insertPartCmd.Parameters.AddWithValue("@Status", (int)part.Status);
                        insertPartCmd.Parameters.AddWithValue("@Gender", (int)part.Gender);
                        insertPartCmd.Parameters.AddWithValue("@DateCreated", now);

                        await insertPartCmd.ExecuteNonQueryAsync();
                    }
                }

                Console.WriteLine("Costume successfully created with parts and linked to choreography.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during costume creation: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateCostumePartAsync(Guid id, UpdatedCostumePartFields updatedFields)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                CostumePart existingCostumePart = null;
                var getExistingCostumePart = @"SELECT * FROM ""CostumePart"" WHERE ""Id"" = @Id";

                using (var existingCommand = new NpgsqlCommand(getExistingCostumePart, connection))
                {
                    existingCommand.Parameters.AddWithValue("@Id", id);

                    using var reader = await existingCommand.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        existingCostumePart = new CostumePart
                        {
                            Id = Guid.Parse(reader["Id"].ToString()),
                            CostumeId = Guid.Parse(reader["CostumeId"].ToString()),
                            Region = reader["Region"]?.ToString(),
                            Name = reader["Name"]?.ToString(),
                            PartNumber = Convert.ToInt32(reader["PartNumber"]),
                            Status = (CostumeStatus)Convert.ToInt32(reader["Status"]),
                            Gender = (Gender)Convert.ToInt32(reader["Gender"]),
                        };
                    }
                    else
                    {
                        Console.WriteLine("Costume part not found.");
                        return false;
                    }
                }

                existingCostumePart.Region = updatedFields.Region ?? existingCostumePart.Region;
                existingCostumePart.Name = updatedFields.Name ?? existingCostumePart.Name;
                existingCostumePart.PartNumber = updatedFields.PartNumber ?? existingCostumePart.PartNumber;
                existingCostumePart.Status = updatedFields.Status ?? existingCostumePart.Status;
                existingCostumePart.Gender = updatedFields.Gender ?? existingCostumePart.Gender;

                var updateQuery = @"
                                    UPDATE ""CostumePart"" SET
                                        ""Region"" = @Region,
                                        ""Name"" = @Name,
                                        ""PartNumber"" = @PartNumber,
                                        ""Status"" = @Status,
                                        ""Gender"" = @Gender
                                    WHERE ""Id"" = @Id;";

                using var updateCmd = new NpgsqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@Region", existingCostumePart.Region ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Name", existingCostumePart.Name ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@PartNumber", existingCostumePart.PartNumber);
                updateCmd.Parameters.AddWithValue("@Status", (int)existingCostumePart.Status);
                updateCmd.Parameters.AddWithValue("@Gender", (int)existingCostumePart.Gender);
                updateCmd.Parameters.AddWithValue("@Id", id);

                int affectedRows = await updateCmd.ExecuteNonQueryAsync();

                if (affectedRows > 0)
                {
                    Console.WriteLine("Costume part successfully updated!");
                }

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating costume part: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> AddCostumePartAsync(Guid costumeId, CostumePart newPart)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var insertPartText = @"INSERT INTO ""CostumePart"" (
                                    ""CostumeId"",
                                    ""Region"",
                                    ""Name"",
                                    ""PartNumber"",
                                    ""Status"",
                                    ""Gender"",
                                    ""DateCreated"")
                               VALUES (
                                    @CostumeId,
                                    @Region,
                                    @Name,
                                    @PartNumber,
                                    @Status,
                                    @Gender,
                                    @DateCreated);";

                using var insertPartCmd = new NpgsqlCommand(insertPartText, connection);

                insertPartCmd.Parameters.AddWithValue("@CostumeId", costumeId);
                insertPartCmd.Parameters.AddWithValue("@Region", newPart.Region ?? (object)DBNull.Value);
                insertPartCmd.Parameters.AddWithValue("@Name", newPart.Name ?? (object)DBNull.Value);
                insertPartCmd.Parameters.AddWithValue("@PartNumber", newPart.PartNumber);
                insertPartCmd.Parameters.AddWithValue("@Status", (int)newPart.Status);
                insertPartCmd.Parameters.AddWithValue("@Gender", (int)newPart.Gender);
                insertPartCmd.Parameters.AddWithValue("@DateCreated", DateTime.UtcNow);

                int affectedRows = await insertPartCmd.ExecuteNonQueryAsync();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding costume part: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteCostumePartAsync(Guid id)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var deleteQuery = @"DELETE FROM ""CostumePart"" WHERE ""Id"" = @Id";

                using var deleteCmd = new NpgsqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@Id", id);

                int affectedRows = await deleteCmd.ExecuteNonQueryAsync();

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting costume part: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteCostumeWithPartsAsync(Guid costumeId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var deletePartsCmd = new NpgsqlCommand(@"DELETE FROM ""CostumePart"" WHERE ""CostumeId"" = @CostumeId", connection);
                deletePartsCmd.Parameters.AddWithValue("@CostumeId", costumeId);
                deletePartsCmd.Transaction = transaction;
                await deletePartsCmd.ExecuteNonQueryAsync();

                var deleteCostumeCmd = new NpgsqlCommand(@"DELETE FROM ""Costume"" WHERE ""Id"" = @CostumeId", connection);
                deleteCostumeCmd.Parameters.AddWithValue("@CostumeId", costumeId);
                deleteCostumeCmd.Transaction = transaction;
                var affected = await deleteCostumeCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return affected > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<Costume>> GetAllCostumesAsync()
        {
            var costumes = new List<Costume>();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandText = @"SELECT * FROM ""Costume"";";
                using var command = new NpgsqlCommand(commandText, connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var costume = new Costume
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        Name = reader["Name"] as string,
                        Area = reader["Area"] as string,
                        Gender = (Gender)reader.GetInt32(reader.GetOrdinal("Gender")),
                        Status = (CostumeStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                        DateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                        DateUpdated = reader.IsDBNull(reader.GetOrdinal("DateUpdated"))
                            ? (DateTime?)null
                            : reader.GetDateTime(reader.GetOrdinal("DateUpdated")),
                        CreatedByUserId = reader.GetGuid(reader.GetOrdinal("CreatedByUserId"))
                    };

                    costumes.Add(costume);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fetching all costumes: " + ex.Message);
            }

            return costumes;
        }

        public async Task<List<CostumePart>> GetAllCostumePartsAsync(Guid costumeId)
        {
            var costumeParts = new List<CostumePart>();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandText = @"SELECT * FROM ""CostumePart"" WHERE ""CostumeId"" = @CostumeId;";
                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@CostumeId", costumeId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var costumePart = new CostumePart
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        CostumeId = reader.GetGuid(reader.GetOrdinal("CostumeId")),
                        Region = reader["Region"] as string,
                        Name = reader["Name"] as string,
                        PartNumber = reader.GetInt32(reader.GetOrdinal("PartNumber")),
                        Status = (CostumeStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                        DateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                    };

                    costumeParts.Add(costumePart);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fetching all costume parts: " + ex.Message);
            }

            return costumeParts;
        }

        public async Task<CostumePart?> GetCostumePartByIdAsync(Guid partId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);

                const string commandText = "SELECT * FROM \"CostumePart\" WHERE \"Id\" = @Id;";
                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid, partId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new CostumePart
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        CostumeId = reader.GetGuid(reader.GetOrdinal("CostumeId")),
                        Region = reader["Region"] as string,
                        Name = reader["Name"] as string,
                        PartNumber = reader.GetInt32(reader.GetOrdinal("PartNumber")),
                        Status = (CostumeStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                        DateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve costume part by ID: " + ex.Message, ex);
            }
        }
    }
}
