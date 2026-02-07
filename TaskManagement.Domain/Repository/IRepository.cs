using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entity;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.IRepository
{
    public interface IRepository
    {
       public Task<List<GetAssignedTaskByUserId>> GetAssignedTasks(int userId);
        public Task<ValidateUserResponce> validateuser(string username, string password);
    }
    /// <summary>
    /// Repository interface for User operations
    /// </summary>
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(int userId);
        Task<List<Role>> GetUserRolesAsync(int userId);
        Task UpdateLastLoginAsync(int userId);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
    }
    
    /// <summary>
    /// Repository interface for RefreshToken operations
    /// </summary>
    public interface IRefreshTokenRepository
    {
        Task<int> InsertAsync(RefreshToken refreshToken);
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<bool> MarkAsUsedAsync(string token, string replacedByToken = null);
        Task<bool> RevokeAsync(string token, string ipAddress, string reason = null, string replacedByToken = null);
        Task<int> RevokeAllUserTokensAsync(int userId, string ipAddress, string reason = null);
        Task<List<RefreshToken>> GetActiveUserTokensAsync(int userId);
        Task<int> CleanupExpiredTokensAsync(int daysOld = 30);
    }
}
