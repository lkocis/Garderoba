using Garderoba.Model;
using Garderoba.Repository.Common;
using Npgsql;

namespace Garderoba.Repository
{
    public class ColaborativeFilteringRepository : IColaborativeFilteringRepository
    {
        private const string _connectionString = "Host=localhost;Port=5433;Username=postgres;Password=PeLana2606;Database=Garderoba";

        private IPerformanceRepository _performanceRepository;
        private IUserRepository _userRepository;

        public ColaborativeFilteringRepository(IPerformanceRepository performanceRepository, IUserRepository userRepository)
        {
            _performanceRepository = performanceRepository;
            _userRepository = userRepository;
        }

        public async Task<Dictionary<Guid, Dictionary<Guid, int>>> FindUserWithCostumePartsAsync(Guid choreographyId, Guid currentUserId)
        {
            try
            {
                var userCostumeParts = await GetUserCostumePartsAsync();
                var partNameToId = await GetPartNameToIdMapAsync();

                var requiredPartsList = await _performanceRepository.GetAllNecessaryPartsByChoreoIdAsync(choreographyId);

                if (!userCostumeParts.ContainsKey(currentUserId))
                {
                    userCostumeParts[currentUserId] = new Dictionary<Guid, int>();

                    foreach (var partName in requiredPartsList)
                    {
                        var trimmedPart = partName.Trim().ToLower();
                        if (partNameToId.TryGetValue(trimmedPart, out Guid partId))
                        {
                            userCostumeParts[currentUserId][partId] = 0;
                        }
                    }
                }

                var similarities = await CalculateUserSimilaritiesAsync(currentUserId, choreographyId);

                var usersWithNeededParts = new Dictionary<Guid, Dictionary<Guid, int>>();

                foreach (var user in similarities.Keys)
                { 
                    if (!userCostumeParts.ContainsKey(user))
                    {
                        userCostumeParts[user] = new Dictionary<Guid, int>();
                    }

                    foreach (var partName in requiredPartsList)
                    {
                        var trimmedPart = partName.Trim().ToLower();
                        if (partNameToId.TryGetValue(trimmedPart, out Guid partId))
                        {
                            if (userCostumeParts[user].TryGetValue(partId, out int quantity) && quantity > 0)
                            {
                                if (!usersWithNeededParts.ContainsKey(user))
                                    usersWithNeededParts[user] = new Dictionary<Guid, int>();

                                usersWithNeededParts[user][partId] = quantity;
                            }
                        }
                    }
                }

                return usersWithNeededParts;
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong: " + ex.Message, ex);
            }
        }


        private async Task<Dictionary<string, Guid>> GetPartNameToIdMapAsync()
        {
            var mapPartNames = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT ""Id"", LOWER(TRIM(""Name"")) AS Name FROM ""CostumePart"";";

            using var command = new NpgsqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var id = reader.GetGuid(reader.GetOrdinal("Id"));
                var name = reader.GetString(reader.GetOrdinal("Name"));

                mapPartNames[name] = id;
            }

            return mapPartNames;
        }

        private async Task<Dictionary<Guid, Dictionary<Guid, int>>> GetUserCostumePartsAsync()
        {
            var mapCostumeParts = new Dictionary<Guid, Dictionary<Guid, int>>();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                        SELECT ""UserId"", ""CostumePartId"", ""Quantity""
                        FROM ""UserCostumePart""
                        WHERE ""Quantity"" > 0;";

            using var command = new NpgsqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var userId = reader.GetGuid(reader.GetOrdinal("UserId"));
                var costumePartId = reader.GetGuid(reader.GetOrdinal("CostumePartId"));
                var quantity = reader.GetInt32(reader.GetOrdinal("Quantity"));

                if(!mapCostumeParts.ContainsKey(userId))
                {
                    mapCostumeParts[userId] = new Dictionary<Guid, int>();
                }


                mapCostumeParts[userId][costumePartId] = quantity;
            }

            return mapCostumeParts;
        }

        private async Task<Dictionary<Guid, double>> CalculateUserSimilaritiesAsync(Guid currentUserId, Guid choreographyId)
        {
            var userCostumeParts = await GetUserCostumePartsAsync();
            // userCostumeParts: Dictionary<UserId, Dictionary<CostumePartId, Quantity>>

            var partNameToId = await GetPartNameToIdMapAsync();

            var requiredPartsList = await _performanceRepository.GetAllNecessaryPartsByChoreoIdAsync(choreographyId);

            if (!userCostumeParts.ContainsKey(currentUserId))
            {
                userCostumeParts[currentUserId] = new Dictionary<Guid, int>();

                foreach (var partName in requiredPartsList)
                {
                    var trimmedPart = partName.Trim().ToLower();
                    if (partNameToId.TryGetValue(trimmedPart, out Guid partId))
                    {
                        userCostumeParts[currentUserId][partId] = 0;
                    }
                }
            }

            var currentVector = userCostumeParts[currentUserId];
            var similarities = new Dictionary<Guid, double>();

            foreach (var user in userCostumeParts.Keys)
            {
                if (user == currentUserId) continue;

                var otherVector = userCostumeParts[user];
                double dotProduct = 0;
                double currentUserNorm = 0;
                double otherUserNorm = 0;

                var allParts = currentVector.Keys.Union(otherVector.Keys);

                foreach (var partId in allParts)
                {
                    currentVector.TryGetValue(partId, out int currentUserQuantity);
                    otherVector.TryGetValue(partId, out int otherUserQuantity);

                    dotProduct += currentUserQuantity * otherUserQuantity;
                    currentUserNorm += currentUserQuantity * currentUserQuantity;
                    otherUserNorm += otherUserQuantity * otherUserQuantity;
                }

                double cosineSimilarity = 0;
                if (currentUserNorm > 0 && otherUserNorm > 0)
                {
                    cosineSimilarity = dotProduct / (Math.Sqrt(currentUserNorm) * Math.Sqrt(otherUserNorm));
                }

                similarities[user] = cosineSimilarity;
            }

            return similarities
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
        }

    }
}
