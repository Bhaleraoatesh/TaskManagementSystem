using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using TaskManagement.Domain.Repository;
using TaskManagement.Domain.Entity;

namespace TaskManagement.Persistance.Repository
{
    public class TaskmanagementRepository : IRepository
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, string> _storedProcedures;

        public TaskmanagementRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _configuration = Guard.Against.Null(configuration, nameof(configuration));
            _connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("TaskManagement"));
            _storedProcedures = _configuration.GetSection("StoredProcedures:TaskmanagementRepository").Get<Dictionary<string, string>>()!;
        }

        public async Task<List<GetAssignedTaskByUserId>> GetAssignedTasks(int userId)
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                var storeProcedure = _storedProcedures.TryGetValue("GetAssignedTasks", out var storedProcedure);
                var result = await connection.QueryAsync<GetAssignedTaskByUserId>(storedProcedure!, new { UserId = userId }, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ValidateUserResponce> validateuser(string username, string password)
        {
             var connection = new SqlConnection(_connectionString);
            var storeProcedure = _storedProcedures.TryGetValue("ValidateUser" ,out var storedProcedure);
            var  result = await connection.QueryAsync<ValidateUserResponce>(storedProcedure!, new { @UserIdentifier = username, @PasswordHash = password }, commandType: CommandType.StoredProcedure);
            return result.FirstOrDefault()!;
        }
    }
}

