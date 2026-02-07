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
    /// Dapper implementation of RefreshToken repository
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public RefreshTokenRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// Insert new refresh token
        /// </summary>
        public async Task<int> InsertAsync(RefreshToken refreshToken)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@Token", refreshToken.Token);
            parameters.Add("@JwtId", refreshToken.JwtId);
            parameters.Add("@UserId", refreshToken.UserId);
            parameters.Add("@ExpiryDate", refreshToken.ExpiryDate);
            parameters.Add("@CreatedByIp", refreshToken.CreatedByIp);
            
            var id = await connection.ExecuteScalarAsync<int>(
                "sp_InsertRefreshToken",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return id;
        }

        /// <summary>
        /// Get refresh token by token string
        /// </summary>
        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var refreshToken = await connection.QueryFirstOrDefaultAsync<RefreshToken>(
                "sp_GetRefreshTokenByToken",
                new { Token = token },
                commandType: CommandType.StoredProcedure
            );

            return refreshToken;
        }

        /// <summary>
        /// Mark refresh token as used
        /// </summary>
        public async Task<bool> MarkAsUsedAsync(string token, string replacedByToken = null)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@Token", token);
            parameters.Add("@ReplacedByToken", replacedByToken);
            
            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_MarkRefreshTokenAsUsed",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result > 0;
        }

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        public async Task<bool> RevokeAsync(
            string token, 
            string ipAddress, 
            string reason = null, 
            string replacedByToken = null)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@Token", token);
            parameters.Add("@RevokedByIp", ipAddress);
            parameters.Add("@ReasonRevoked", reason);
            parameters.Add("@ReplacedByToken", replacedByToken);
            
            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_RevokeRefreshToken",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result > 0;
        }

        /// <summary>
        /// Revoke all active refresh tokens for a user
        /// </summary>
        public async Task<int> RevokeAllUserTokensAsync(
            int userId, 
            string ipAddress, 
            string reason = null)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@RevokedByIp", ipAddress);
            parameters.Add("@ReasonRevoked", reason);
            
            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_RevokeAllUserRefreshTokens",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        /// <summary>
        /// Get all active refresh tokens for a user
        /// </summary>
        public async Task<List<RefreshToken>> GetActiveUserTokensAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var tokens = await connection.QueryAsync<RefreshToken>(
                "sp_GetActiveUserRefreshTokens",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            );

            return tokens.ToList();
        }

        /// <summary>
        /// Clean up expired refresh tokens
        /// </summary>
        public async Task<int> CleanupExpiredTokensAsync(int daysOld = 30)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "sp_CleanupExpiredRefreshTokens",
                new { DaysOld = daysOld },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
    }
}