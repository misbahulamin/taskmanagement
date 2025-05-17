using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TaskManagement.Services
{
    public class RecurringTaskBackgroundService : BackgroundService
    {
        private readonly ILogger<RecurringTaskBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        public RecurringTaskBackgroundService(
            ILogger<RecurringTaskBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring Task Background Service is running.");
            
            // Run once at startup
            await GenerateRecurringTasksAsync();
            
            // Then run once daily at midnight
            while (!stoppingToken.IsCancellationRequested)
            {
                // Calculate time until next midnight
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1);
                var delay = nextRun - now;
                
                _logger.LogInformation($"Next recurring task check scheduled for {nextRun.ToString("yyyy-MM-dd HH:mm:ss")}");
                
                try
                {
                    // Wait until next execution time
                    await Task.Delay(delay, stoppingToken);
                    
                    // Generate recurring tasks
                    await GenerateRecurringTasksAsync();
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while generating recurring tasks");
                }
            }
            
            _logger.LogInformation("Recurring Task Background Service is stopping.");
        }
        
        private async Task GenerateRecurringTasksAsync()
        {
            _logger.LogInformation("Checking for recurring tasks to generate...");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var recurringTaskService = scope.ServiceProvider.GetRequiredService<IRecurringTaskService>();
                
                var generatedTasks = await recurringTaskService.GeneratePendingRecurringTasksAsync();
                
                _logger.LogInformation($"Generated {generatedTasks.Count} recurring tasks.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating recurring tasks");
            }
        }
    }
} 