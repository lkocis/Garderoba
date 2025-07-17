using Garderoba.Model;
using Garderoba.Repository.Common;
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
    }
}
