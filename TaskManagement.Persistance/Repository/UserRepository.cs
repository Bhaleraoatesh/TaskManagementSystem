using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.IRepository;

namespace TaskManagement.Persistance.Repositories
{
    /// <summary>
    /// Dapper implementation of User repository
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// Get user by email with roles
        /// </summary>
        public async Task<User> GetByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // Get user
            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "sp_GetUserByEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure
            );

            if (user != null)
            {
                // Get user roles
                user.Roles = (await GetUserRolesAsync(user.Id)).ToList();
            }

            return user;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<User> GetByIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT
                    Id, Email, PasswordHash, FirstName, LastName, FullName, IsActive,
                    CreatedDate, LastLoginDate, LastPasswordChangeDate
                FROM Users
                WHERE Id = @UserId";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });

            if (user != null)
            {
                user.Roles = (await GetUserRolesAsync(userId)).ToList();
            }

            return user;
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        public async Task<List<Role>> GetUserRolesAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var roles = await connection.QueryAsync<Role>(
                "sp_GetUserRoles",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            );

            return roles.ToList();
        }

        /// <summary>
        /// Update user's last login date
        /// </summary>
        public async Task UpdateLastLoginAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            await connection.ExecuteAsync(
                "sp_UpdateUserLastLogin",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Create new user
        /// </summary>
        public async Task<bool> CreateUserAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                INSERT INTO Users
                (Email, PasswordHash, FirstName, LastName, IsActive, CreatedDate)
                VALUES
                (@Email, @PasswordHash, @FirstName, @LastName, @IsActive, GETUTCDATE());

                SELECT CAST(SCOPE_IDENTITY() as int);";

            user.Id = await connection.ExecuteScalarAsync<int>(sql, user);
            return user.Id > 0;
        }

        /// <summary>
        /// Update existing user
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE Users
                SET
                    Email = @Email,
                    FirstName = @FirstName,
                    LastName = @LastName,
                    IsActive = @IsActive,
                    PasswordHash = @PasswordHash
                WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, user);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Check if user exists by email
        /// </summary>
        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT COUNT(1)
                FROM Users
                WHERE Email = @Email";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }
    }
}