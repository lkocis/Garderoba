using Garderoba.Model;
using Garderoba.Repository.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Npgsql;

namespace Garderoba.Repository
{
    public class UserRepository : IUserRepository
    {
        private const string _connectionString = "Host=localhost;Port=5433;Username=postgres;Password=PeLana2606;Database=Garderoba";

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var commandTextEmail = "SELECT * FROM \"User\" WHERE \"Email\" = @Email;";
                var checkCommand = new NpgsqlCommand(commandTextEmail, connection);
                checkCommand.Parameters.AddWithValue("@Email", user.Email);

                var exists = false;
                using (var reader = await checkCommand.ExecuteReaderAsync())
                {
                    exists = await reader.ReadAsync();
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                var commandText = @"
                INSERT INTO ""User"" (
                    ""Email"",
                    ""Password"",
                    ""FirstName"",
                    ""LastName"",
                    ""PhoneNumber"",
                    ""Area"",
                    ""KUDName"",
                    ""DateCreated"",
                    ""DateUpdated"")
                VALUES (
                    @Email,
                    @Password,
                    @FirstName,
                    @LastName,
                    @PhoneNumber,
                    @Area,
                    @KUDName,
                    @DateCreated,
                    @DateUpdated
                );";

                using var insertCommand = new NpgsqlCommand(commandText, connection);

                insertCommand.Parameters.AddWithValue("@Email", user.Email);
                insertCommand.Parameters.AddWithValue("@Password", hashedPassword);
                insertCommand.Parameters.AddWithValue("@FirstName", user.FirstName ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@LastName", user.LastName ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Area", user.Area ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@KUDName", user.KUDName ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@DateCreated", DateTime.UtcNow);
                insertCommand.Parameters.AddWithValue("@DateUpdated", DBNull.Value);

                int numberOfCommits = await insertCommand.ExecuteNonQueryAsync();

                Console.WriteLine(numberOfCommits > 0 ? "User successfully registered!" : "User registration failed.");
                return numberOfCommits > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during registration: " + ex.Message);
                return false;
            }
        }

        public async Task<User?> ReadUserAsync(Guid id)
        {
            try
            {
                User? user = new User();
                using var connection = new NpgsqlConnection(_connectionString);
                string commandText = $"SELECT * FROM \"User\" WHERE \"Id\" = @Id;";

                using var command = new NpgsqlCommand(commandText, connection);

                command.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid, id);

                await connection.OpenAsync();
                using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                await reader.ReadAsync();

                user.Id = Guid.Parse(reader["Id"].ToString());
                user.Email = reader["Email"].ToString();
                user.Password = reader["Password"].ToString();
                user.FirstName = reader["FirstName"].ToString();
                user.LastName = reader["LastName"].ToString();
                user.PhoneNumber = reader["PhoneNumber"].ToString();
                user.Area = reader["Area"].ToString();
                user.KUDName = reader["KUDName"].ToString();
                user.DateCreated = Convert.ToDateTime(reader["DateCreated"]);
                user.DateUpdated = reader["DateUpdated"] != DBNull.Value ? Convert.ToDateTime(reader["DateUpdated"]) : null;

                return user;
            }
            catch(Exception ex) 
            {
                Console.WriteLine("User with that id doesn't exist: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> LoginUserAsync(string email, string password)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                string commandText = "SELECT * FROM \"User\" WHERE \"Email\" = @Email;";

                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@Email", email);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    Console.WriteLine($"Wrong email or password");
                    return null;
                }

                string hashedPassword = reader["Password"].ToString();
                if (!BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                {
                    Console.WriteLine("Wrong password!");
                    return null;
                }

                var user = new User
                {
                    Id = Guid.Parse(reader["Id"].ToString()),
                    Email = reader["Email"].ToString(),
                    Password = hashedPassword,
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString(),
                    Area = reader["Area"].ToString(),
                    KUDName = reader["KUDName"].ToString(),
                    DateCreated = Convert.ToDateTime(reader["DateCreated"]),
                    DateUpdated = reader["DateUpdated"] == DBNull.Value ? null : Convert.ToDateTime(reader["DateUpdated"])
                };

                return user;
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Unsucessful login: " +  ex.Message );
                return null;
            }
        }

    }
}
