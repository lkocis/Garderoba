using Garderoba.Model;
using Garderoba.Repository.Common;
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
                                        ""IsAvailable"", 
                                        ""DateCreated"", 
                                        ""CreatedByUserId"")
                                    VALUES (
                                        @Name, 
                                        @Area, 
                                        @Gender, 
                                        @IsAvailable, 
                                        @DateCreated, 
                                        @CreatedByUserId)
                                    RETURNING ""Id"";";

                using var insertCostumeCmd = new NpgsqlCommand(insertCostumeText, connection);

                insertCostumeCmd.Parameters.AddWithValue("@Name", costume.Name ?? (object)DBNull.Value);
                insertCostumeCmd.Parameters.AddWithValue("@Area", costume.Area ?? (object)DBNull.Value);
                insertCostumeCmd.Parameters.AddWithValue("@Gender", (int)costume.Gender);
                insertCostumeCmd.Parameters.AddWithValue("@Status", costume.Status);
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
                        insertPartCmd.Parameters.AddWithValue("@Status", part.Status);
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
    }
}
