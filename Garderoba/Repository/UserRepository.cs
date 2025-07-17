using Garderoba.Model;
using Garderoba.Repository.Common;
using Garderoba.WebApi.ViewModel;
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

                if (exists)
                {
                    Console.WriteLine("User with this email already exists.");
                    return false;
                }

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
                insertCommand.Parameters.AddWithValue("@Password", user.Password);
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

        public async Task<bool> UpdateUserAsync(Guid id, UpdatedUserInfoFields updatedUser)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                User existingUser = null;
                using (var getUserCmd = new NpgsqlCommand("SELECT * FROM \"User\" WHERE \"Id\" = @Id", connection))
                {
                    getUserCmd.Parameters.AddWithValue("@Id", id);

                    using var reader = await getUserCmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        existingUser = new User
                        {
                            Id = Guid.Parse(reader["Id"].ToString()),
                            Email = reader["Email"]?.ToString(),
                            FirstName = reader["FirstName"]?.ToString(),
                            LastName = reader["LastName"]?.ToString(),
                            PhoneNumber = reader["PhoneNumber"]?.ToString(),
                            Area = reader["Area"]?.ToString(),
                            KUDName = reader["KUDName"]?.ToString()
                        };
                    }
                    else
                    {
                        Console.WriteLine("User not found.");
                        return false;
                    }
                } 

                existingUser.Email = updatedUser.Email ?? existingUser.Email;
                existingUser.FirstName = updatedUser.FirstName ?? existingUser.FirstName;
                existingUser.LastName = updatedUser.LastName ?? existingUser.LastName;
                existingUser.PhoneNumber = updatedUser.PhoneNumber ?? existingUser.PhoneNumber;
                existingUser.Area = updatedUser.Area ?? existingUser.Area;
                existingUser.KUDName = updatedUser.KUDName ?? existingUser.KUDName;

                using var updateCmd = new NpgsqlCommand(@"
                                                UPDATE ""User"" SET
                                                        ""Email"" = @Email,
                                                        ""FirstName"" = @FirstName,
                                                        ""LastName"" = @LastName,
                                                        ""PhoneNumber"" = @PhoneNumber,
                                                        ""Area"" = @Area,
                                                        ""KUDName"" = @KUDName,
                                                        ""DateUpdated"" = @DateUpdated
                                                WHERE ""Id"" = @Id;", connection);

                updateCmd.Parameters.AddWithValue("@Email", existingUser.Email ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@FirstName", existingUser.FirstName ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@LastName", existingUser.LastName ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@PhoneNumber", existingUser.PhoneNumber ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Area", existingUser.Area ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@KUDName", existingUser.KUDName ?? (object)DBNull.Value);
                updateCmd.Parameters.AddWithValue("@DateUpdated", DateTime.UtcNow);
                updateCmd.Parameters.AddWithValue("@Id", id);

                int affectedRows = await updateCmd.ExecuteNonQueryAsync();

                if(affectedRows > 0)
                {
                    Console.WriteLine("User info fields successfully updated!");
                }

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating user: " + ex.Message);
                return false;
            }
        }

        public async Task<User?> LoginUserAsync(string email, string password)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                string commandText = "SELECT * FROM \"User\" WHERE \"Email\" = @Email AND \"Password\" = @Password;";

                using var command = new NpgsqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    Console.WriteLine($"Wrong email or password");
                    return null;
                }

                var user = new User
                {
                    Id = Guid.Parse(reader["Id"].ToString()),
                    Email = reader["Email"].ToString(),
                    Password = reader["Password"].ToString(),
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
