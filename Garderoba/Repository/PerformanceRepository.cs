using Garderoba.Repository.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

namespace Garderoba.Repository
{
    public class PerformanceRepository : IPerformanceRepository
    {
        private const string _connectionString = "Host=localhost;Port=5433;Username=postgres;Password=PeLana2606;Database=Garderoba";

        public async Task<int> GetMenCostumeCountAsync(Guid choreographyId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandText = @"
                                SELECT ""MenCostumeCount""
                                FROM ""Choreography""
                                WHERE ""Id"" = @ChoreographyId;";

                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@ChoreographyId", choreographyId);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving MenCostumeCount: " + ex.Message, ex);
            }
        }

        public async Task<List<Guid>> GetMaleCostumeIdsByChoreoIdAsync(Guid choreographyId)
        {
            var costumeIds = new List<Guid>();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandText = @"
                                SELECT c.""Id"" AS CostumeId
                                FROM ""ChoreographyCostume"" cc
                                JOIN ""Costume"" c ON cc.""CostumeId"" = c.""Id""
                                WHERE cc.""ChoreographyId"" = @ChoreographyId AND c.""Gender"" = 0;";

                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@ChoreographyId", choreographyId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    costumeIds.Add(reader.GetGuid(reader.GetOrdinal("CostumeId")));
                }

                return costumeIds;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving male costume IDs: " + ex.Message, ex);
            }
        }

        public async Task<List<string>> GetNecessaryPartsListAsync(Guid costumeId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                            SELECT c.""NecessaryParts""
                            FROM ""Costume"" c
                            WHERE c.""Id"" = @CostumeId;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@CostumeId", costumeId);

                var necessaryParts = (string?)await command.ExecuteScalarAsync() ?? string.Empty;

                var partsList = necessaryParts
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                return partsList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving necessary parts list: " + ex.Message, ex);
            }
        }

        public async Task<bool> CompareNecessaryAndActualPartsAsync(Guid costumeId)
        {
            try
            {
                var necessaryParts = await GetNecessaryPartsListAsync(costumeId);

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                            SELECT cp.""Name""
                            FROM ""CostumePart"" cp
                            WHERE cp.""CostumeId"" = @CostumeId AND cp.""Gender"" = 0;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@CostumeId", costumeId);

                var actualParts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var partName = reader["Name"]?.ToString()?.Trim().ToLower();
                    if (!string.IsNullOrEmpty(partName))
                    {
                        actualParts.Add(partName);
                    }
                }

                foreach (var part in necessaryParts)
                {
                    if (!actualParts.Contains(part))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error comparing necessary and actual parts: " + ex.Message, ex);
            }
        }

        public async Task<(bool AllPartsAvailable, List<string> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId)
        {
            var missingParts = new List<string>();

            try
            {
                int menCostumeCount = await GetMenCostumeCountAsync(choreographyId);
                var maleCostumeIds = await GetMaleCostumeIdsByChoreoIdAsync(choreographyId);

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                foreach (var costumeId in maleCostumeIds)
                {
                    var necessaryParts = await GetNecessaryPartsListAsync(costumeId);

                    var partCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                    var query = @"
                                SELECT cp.""Name"", cp.""PartNumber""
                                FROM ""CostumePart"" cp
                                WHERE cp.""CostumeId"" = @CostumeId AND cp.""Gender"" = 0;";

                    using var command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CostumeId", costumeId);

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var partName = reader["Name"]?.ToString()?.Trim().ToLower();
                        var partNumber = reader["PartNumber"] != DBNull.Value ? Convert.ToInt32(reader["PartNumber"]) : 0;

                        if (!string.IsNullOrEmpty(partName))
                            partCounts[partName] = partNumber;
                    }
                    await reader.CloseAsync();

                    foreach (var necessaryPart in necessaryParts)
                    {
                        if (!partCounts.ContainsKey(necessaryPart))
                        {
                            missingParts.Add($"{necessaryPart} (nedostaje svih {menCostumeCount})");
                        }
                        else if (partCounts[necessaryPart] < menCostumeCount)
                        {
                            int missingCount = menCostumeCount - partCounts[necessaryPart];
                            missingParts.Add($"{necessaryPart} (nedostaje još {missingCount})");
                        }
                    }
                }

                bool allAvailable = missingParts.Count == 0;
                return (allAvailable, missingParts);
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking stock and listing missing costume parts: " + ex.Message, ex);
            }
        }


    }
}
