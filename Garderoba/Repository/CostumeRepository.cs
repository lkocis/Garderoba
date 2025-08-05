using Garderoba.Common;
using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.WebApi.ViewModel;
using Npgsql;

namespace Garderoba.Repository
{
    public class CostumeRepository : ICostumeRepository
    {
        private const string _connectionString = "Host=localhost;Port=5433;Username=postgres;Password=PeLana2606;Database=Garderoba";

        public async Task<bool> CreateNewCostumeAsync(Costume costume)
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
                                        ""DateCreated"", 
                                        ""CreatedByUserId"")
                                    VALUES (
                                        @Name, 
                                        @Area, 
                                        @Gender, 
                                        @Status, 
                                        @DateCreated, 
                                        @CreatedByUserId)
                                    RETURNING ""Id"";";

                using var insertCostumeCmd = new NpgsqlCommand(insertCostumeText, connection);

                insertCostumeCmd.Parameters.AddWithValue("@Name", costume.Name ?? (object)DBNull.Value);
                insertCostumeCmd.Parameters.AddWithValue("@Area", costume.Area ?? (object)DBNull.Value);
                insertCostumeCmd.Parameters.AddWithValue("@Gender", (int)costume.Gender);
                insertCostumeCmd.Parameters.AddWithValue("@Status", (int)costume.Status);
                insertCostumeCmd.Parameters.AddWithValue("@DateCreated", now);
                insertCostumeCmd.Parameters.AddWithValue("@CreatedByUserId", costume.CreatedByUserId);

                var costumeId = (Guid)await insertCostumeCmd.ExecuteScalarAsync();

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
                                            ""DateCreated"")
                                        VALUES (
                                            @CostumeId, 
                                            @Region, 
                                            @Name, 
                                            @PartNumber, 
                                            @Status, 
                                            @DateCreated);";

                        using var insertPartCmd = new NpgsqlCommand(insertPartText, connection);
                        insertPartCmd.Parameters.AddWithValue("@CostumeId", costumeId);
                        insertPartCmd.Parameters.AddWithValue("@Region", part.Region ?? (object)DBNull.Value);
                        insertPartCmd.Parameters.AddWithValue("@Name", part.Name ?? (object)DBNull.Value);
                        insertPartCmd.Parameters.AddWithValue("@PartNumber", part.PartNumber);
                        insertPartCmd.Parameters.AddWithValue("@Status", (int)part.Status);
                        insertPartCmd.Parameters.AddWithValue("@DateCreated", now);

                        await insertPartCmd.ExecuteNonQueryAsync();
                    }
                }

                Console.WriteLine("Costume successfully created with parts.");
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

                var updateQuery = @"
                                    UPDATE ""CostumePart"" SET
                                        ""Region"" = @Region,
                                        ""Name"" = @Name,
                                        ""PartNumber"" = @PartNumber,
                                        ""Status"" = @Status
                                    WHERE ""Id"" = @Id;";

                using var updateCmd = new NpgsqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@Region", existingCostumePart.Region ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Name", existingCostumePart.Name ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@PartNumber", existingCostumePart.PartNumber);
                updateCmd.Parameters.AddWithValue("@Status", (int)existingCostumePart.Status);  
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


    }
}
