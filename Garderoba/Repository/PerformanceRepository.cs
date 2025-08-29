using Garderoba.Common;
using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.WebApi.ViewModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;

namespace Garderoba.Repository
{
    public class PerformanceRepository : IPerformanceRepository
    {
        private const string _connectionString = "Host=localhost;Port=5433;Username=postgres;Password=PeLana2606;Database=Garderoba";

        public async Task<int> GetCostumeCountAsync(Guid choreographyId, int gender)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                if(gender == 0)
                {
                    var commandText = @"
                                SELECT ""MenCostumeCount""
                                FROM ""Choreography""
                                WHERE ""Id"" = @ChoreographyId;";

                    using var command = new NpgsqlCommand(commandText, connection);
                    command.Parameters.AddWithValue("@ChoreographyId", choreographyId);

                    var result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
                else
                {
                    var commandText = @"
                                SELECT ""WomenCostumeCount""
                                FROM ""Choreography""
                                WHERE ""Id"" = @ChoreographyId;";

                    using var command = new NpgsqlCommand(commandText, connection);
                    command.Parameters.AddWithValue("@ChoreographyId", choreographyId);

                    var result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving CostumeCount: " + ex.Message, ex);
            }
        }

        public async Task<List<Guid>> GetCostumeIdsByChoreoIdAsync(Guid choreographyId, int gender, Guid userId)
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
            WHERE cc.""ChoreographyId"" = @ChoreographyId 
              AND c.""Gender"" = @Gender
              AND c.""CreatedByUserId"" = @UserId;";

                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@ChoreographyId", choreographyId);
                command.Parameters.AddWithValue("@Gender", gender);
                command.Parameters.AddWithValue("@UserId", userId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    costumeIds.Add(reader.GetGuid(reader.GetOrdinal("CostumeId")));
                }

                return costumeIds;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving costume IDs: " + ex.Message, ex);
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

                var result = await command.ExecuteScalarAsync();
                var necessaryParts = result != DBNull.Value ? result.ToString() : string.Empty;

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

        public async Task<(bool AllPartsAvailable, List<MissingPartsVM> MissingParts)> CheckIfAllNecessaryPartsInStockWithMissingListAsync(Guid choreographyId, Guid userId)
        {
            var missingParts = new List<MissingPartsVM>();

            try
            {
                int menCostumeCount = await GetCostumeCountAsync(choreographyId, gender: 0);
                var maleCostumeIds = await GetCostumeIdsByChoreoIdAsync(choreographyId, gender: 0, userId);

                int womenCostumeCount = await GetCostumeCountAsync(choreographyId, gender: 1);
                var femaleCostumeIds = await GetCostumeIdsByChoreoIdAsync(choreographyId, gender: 1, userId);

                await CheckPartsAsync(maleCostumeIds, menCostumeCount, 0, missingParts, choreographyId);
                await CheckPartsAsync(femaleCostumeIds, womenCostumeCount, 1, missingParts, choreographyId);

                bool allAvailable = missingParts.Count == 0;
                return (allAvailable, missingParts);
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking stock and listing missing costume parts: " + ex.Message, ex);
            }
        }

        private async Task CheckPartsAsync(List<Guid> costumeIds, int costumeCount, int gender, List<MissingPartsVM> missingParts, Guid choreoId)
        {
            if (costumeIds == null || costumeIds.Count == 0)
            {
                var allParts = await GetNecessaryPartsByChoreoIdAsync(choreoId, gender);

                foreach (var part in allParts)
                {
                    missingParts.Add(new MissingPartsVM
                    {
                        Name = part,
                        PartNumber = costumeCount,
                        Gender = (Gender)gender
                    });
                }

                return;
            }

            foreach (var costumeId in costumeIds)
            {
                var necessaryParts = await GetNecessaryPartsListAsync(costumeId);

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var partCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                var query = @"
                            SELECT cp.""Name"", cp.""PartNumber""
                            FROM ""CostumePart"" cp
                            WHERE cp.""CostumeId"" = @CostumeId;";

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

                foreach (var necessaryPart in necessaryParts)
                {
                    partCounts.TryGetValue(necessaryPart, out int availableCount);

                    if (availableCount < costumeCount)
                    {
                        missingParts.Add(new MissingPartsVM
                        {
                            Name = necessaryPart,
                            PartNumber = costumeCount - availableCount,
                            Gender = (Gender)gender
                        });
                    }
                }

                missingParts = missingParts
                    .GroupBy(mp => new { mp.Name, mp.Gender })
                    .Select(g => new MissingPartsVM
                    {
                        Name = g.Key.Name,
                        Gender = g.Key.Gender,
                        PartNumber = g.Sum(x => x.PartNumber)
                    })
                    .ToList();
            }
        }

        private async Task<List<string>> GetNecessaryPartsByChoreoIdAsync(Guid choreographyId, int gender)
        {
            var necessaryPartsList = new List<string>();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
        SELECT DISTINCT c.""NecessaryParts""
        FROM ""Costume"" c
        JOIN ""ChoreographyCostume"" cc ON cc.""CostumeId"" = c.""Id""
        WHERE cc.""ChoreographyId"" = @ChoreographyId AND c.""Gender"" = @Gender;";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@ChoreographyId", choreographyId);
            command.Parameters.AddWithValue("@Gender", gender);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var partsString = reader["NecessaryParts"]?.ToString();
                if (!string.IsNullOrEmpty(partsString))
                    necessaryPartsList.AddRange(partsString.Split(',', StringSplitOptions.TrimEntries));
            }

            return necessaryPartsList.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

    }
}
