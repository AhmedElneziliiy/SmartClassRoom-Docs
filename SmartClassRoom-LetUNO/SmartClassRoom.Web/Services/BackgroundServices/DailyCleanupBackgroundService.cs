using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.BackgroundServices;

/// <summary>
/// Background service that runs daily cleanup at a scheduled time
/// </summary>
public class DailyCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyCleanupBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public DailyCleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DailyCleanupBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Cleanup Background Service is starting");

        // Get cleanup time from configuration (default: 23:59 / 11:59 PM)
        var cleanupTimeStr = _configuration["CleanupSettings:DailyCleanupTime"] ?? "23:59";
        if (!TimeSpan.TryParse(cleanupTimeStr, out var cleanupTime))
        {
            cleanupTime = new TimeSpan(23, 59, 0); // Default to 11:59 PM
        }

        _logger.LogInformation("Daily cleanup scheduled for {CleanupTime}", cleanupTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var nextCleanup = CalculateNextCleanupTime(now, cleanupTime);
                var delay = nextCleanup - now;

                _logger.LogInformation(
                    "Next cleanup scheduled at {NextCleanup} (in {DelayHours:F2} hours)",
                    nextCleanup, delay.TotalHours);

                // Wait until the scheduled time
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await PerformCleanupAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Daily Cleanup Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Daily Cleanup Background Service");
                // Wait 1 hour before retrying in case of error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task PerformCleanupAsync()
    {
        _logger.LogInformation("Starting scheduled daily cleanup");

        try
        {
            // Create a new scope for the cleanup operation
            using var scope = _serviceProvider.CreateScope();
            var cleanupService = scope.ServiceProvider.GetRequiredService<IDailyCleanupService>();

            var summary = await cleanupService.PerformDailyCleanupAsync();

            if (summary.Errors.Any())
            {
                _logger.LogWarning(
                    "Daily cleanup completed with errors. Sessions: {Sessions}, Students: {Students}, Teachers: {Teachers}, Errors: {Errors}",
                    summary.SessionsEnded, summary.StudentCheckoutsForced, summary.TeacherCheckoutsForced, string.Join(", ", summary.Errors));
            }
            else
            {
                _logger.LogInformation(
                    "Daily cleanup completed successfully. Sessions ended: {Sessions}, Student checkouts: {Students}, Teacher checkouts: {Teachers}",
                    summary.SessionsEnded, summary.StudentCheckoutsForced, summary.TeacherCheckoutsForced);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform daily cleanup");
        }
    }

    private static DateTime CalculateNextCleanupTime(DateTime now, TimeSpan cleanupTime)
    {
        var today = now.Date;
        var todayCleanup = today.Add(cleanupTime);

        // If today's cleanup time has passed, schedule for tomorrow
        if (now >= todayCleanup)
        {
            return today.AddDays(1).Add(cleanupTime);
        }

        return todayCleanup;
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Cleanup Background Service is stopping");
        return base.StopAsync(stoppingToken);
    }
}
