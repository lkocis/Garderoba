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

        private async Task<Guid> InsertCostumeAsync(NpgsqlConnection connection, Costume costume, DateTime now)
        {
            var query = @"INSERT INTO ""Costume"" (
                                ""Name"", ""Area"", ""Gender"", ""Status"", ""NecessaryParts"",
                                ""DateCreated"", ""CreatedByUserId"")
                            VALUES (
                                @Name, @Area, @Gender, @Status, @NecessaryParts,
                                @DateCreated, @CreatedByUserId)
                            RETURNING ""Id"";";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Name", costume.Name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Area", costume.Area ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", (int)costume.Gender);
            cmd.Parameters.AddWithValue("@Status", (int)costume.Status);
            cmd.Parameters.AddWithValue("@NecessaryParts", costume.NecessaryParts ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DateCreated", now);
            cmd.Parameters.AddWithValue("@CreatedByUserId", costume.CreatedByUserId);

            return (Guid)await cmd.ExecuteScalarAsync();
        }

        private async Task LinkCostumeToChoreographyAsync(NpgsqlConnection connection, Guid costumeId, Guid choreographyId)
        {
            var query = @"INSERT INTO ""ChoreographyCostume"" (""ChoreographyId"", ""CostumeId"")
                            VALUES (@ChoreographyId, @CostumeId);";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@ChoreographyId", choreographyId);
            cmd.Parameters.AddWithValue("@CostumeId", costumeId);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<Guid> InsertCostumePartAsync(NpgsqlConnection connection, Guid costumeId, CostumePart part, DateTime now)
        {
            var query = @"INSERT INTO ""CostumePart"" (
                                ""CostumeId"", ""Region"", ""Name"", ""PartNumber"",
                                ""Status"", ""Gender"", ""DateCreated"")
                            VALUES (
                                @CostumeId, @Region, @Name, @PartNumber,
                                @Status, @Gender, @DateCreated)
                            RETURNING ""Id"";";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@CostumeId", costumeId);
            cmd.Parameters.AddWithValue("@Region", part.Region ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Name", part.Name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PartNumber", part.PartNumber);
            cmd.Parameters.AddWithValue("@Status", (int)part.Status);
            cmd.Parameters.AddWithValue("@Gender", (int)part.Gender);
            cmd.Parameters.AddWithValue("@DateCreated", now);

            return (Guid)await cmd.ExecuteScalarAsync();
        }

        private async Task InsertUserCostumePartAsync(NpgsqlConnection connection, Guid userId, Guid costumePartId, int quantity, DateTime now)
        {
            var query = @"INSERT INTO ""UserCostumePart"" (
                                ""Id"", ""UserId"", ""CostumePartId"", ""Quantity"", ""DateCreated"")
                            VALUES (
                                @Id, @UserId, @CostumePartId, @Quantity, @DateCreated);";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@CostumePartId", costumePartId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            cmd.Parameters.AddWithValue("@DateCreated", now);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> CreateNewCostumeAsync(Costume costume, Guid? choreographyId, Guid userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var exists = await CheckCostumeExistsAsync(connection, costume.Name, userId);
                if (exists) return false;

                var now = DateTime.UtcNow;
                costume.CreatedByUserId = userId;
                var costumeId = await InsertCostumeAsync(connection, costume, now);

                if (choreographyId.HasValue)
                    await LinkCostumeToChoreographyAsync(connection, costumeId, choreographyId.Value);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CheckCostumeExistsAsync(NpgsqlConnection connection, string costumeName, Guid userId)
        {
            try
            {
                var query = @"SELECT 1 FROM ""Costume"" WHERE ""Name"" = @Name AND ""CreatedByUserId"" = @UserId LIMIT 1;";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Name", costumeName);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using var reader = await cmd.ExecuteReaderAsync();
                return await reader.ReadAsync();
            }
            catch
            {
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

        public async Task<bool> AddCostumePartAsync(CostumePart newPart, Guid costumeId)
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

        public async Task<List<Costume>> GetAllCostumesAsync(Guid userId, Guid choreographyId)
        {
            var costumes = new List<Costume>();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandText = @"
            SELECT 
                c.""Id"" AS ""CostumeId"",
                c.""Name"",
                c.""Area"",
                c.""Gender"",
                c.""Status"",
                c.""NecessaryParts"",
                c.""DateCreated"",
                c.""DateUpdated"",
                c.""CreatedByUserId"",
                u.""Id"" AS ""UserId"",
                u.""Email"",
                u.""FirstName"",
                u.""LastName"",
                u.""PhoneNumber"",
                u.""Area"" AS ""UserArea"",
                u.""KUDName"",
                u.""DateCreated"" AS ""UserDateCreated"",
                u.""DateUpdated"" AS ""UserDateUpdated""
            FROM ""Costume"" c
            JOIN ""User"" u ON c.""CreatedByUserId"" = u.""Id""
            JOIN ""ChoreographyCostume"" cc ON c.""Id"" = cc.""CostumeId""
            WHERE c.""CreatedByUserId"" = @UserId AND cc.""ChoreographyId"" = @ChoreographyId
            ORDER BY c.""Name"";";

                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@ChoreographyId", choreographyId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var costume = new Costume
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("CostumeId")),
                        Name = reader["Name"] as string,
                        Area = reader["Area"] as string,
                        Gender = (Gender)reader.GetInt32(reader.GetOrdinal("Gender")),
                        Status = (CostumeStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                        NecessaryParts = reader["NecessaryParts"] as string,
                        DateCreated = reader.IsDBNull(reader.GetOrdinal("DateCreated")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                        DateUpdated = reader.IsDBNull(reader.GetOrdinal("DateUpdated")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DateUpdated")),
                        CreatedByUserId = reader.GetGuid(reader.GetOrdinal("CreatedByUserId")),

                        CreatedByUser = new User
                        {
                            Id = reader.GetGuid(reader.GetOrdinal("UserId")),
                            Email = reader["Email"] as string,
                            FirstName = reader["FirstName"] as string,
                            LastName = reader["LastName"] as string,
                            PhoneNumber = reader["PhoneNumber"] as string,
                            Area = reader["UserArea"] as string,
                            KUDName = reader["KUDName"] as string,
                            DateCreated = reader.IsDBNull(reader.GetOrdinal("UserDateCreated")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UserDateCreated")),
                            DateUpdated = reader.IsDBNull(reader.GetOrdinal("UserDateUpdated")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UserDateUpdated"))
                        }
                    };

                    costumes.Add(costume);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fetching costumes for user: " + ex.Message);
            }

            return costumes;
        }

        public async Task<List<CostumePart>> GetAllCostumePartsAsync(Guid userId, Guid costumeId)
        {
            var costumeParts = new List<CostumePart>();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandText = @"
                                    SELECT cp.*
                                    FROM ""CostumePart"" cp
                                    JOIN ""Costume"" c ON cp.""CostumeId"" = c.""Id""
                                    WHERE cp.""CostumeId"" = @CostumeId AND c.""CreatedByUserId"" = @UserId;
                                ";

                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@CostumeId", costumeId);
                command.Parameters.AddWithValue("@UserId", userId);

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
