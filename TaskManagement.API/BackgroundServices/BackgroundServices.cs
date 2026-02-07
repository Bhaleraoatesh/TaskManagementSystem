using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.BackgroundServices
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);

        public TokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokensAsync(stoppingToken);
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up tokens");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Token Cleanup Service stopped");
        }

        private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting token cleanup");

            using var scope = _serviceProvider.CreateScope();
            var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

            try
            {
                await jwtTokenService.CleanupExpiredTokensAsync();
                _logger.LogInformation("Token cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token cleanup failed");
            }
        }
    }
}