using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Data;
using UptimeMonitor.Models;

namespace UptimeMonitor.Services;

public class CheckRunner : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CheckRunner> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    public CheckRunner(
        IServiceProvider services,
        ILogger<CheckRunner> logger,
        IHttpClientFactory httpClientFactory)
    {
        _services = services;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CheckRunner started (every {Interval} min)", Interval.TotalMinutes);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RunChecksAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // TODO: Graceful Shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running checks");
            }
        }

        _logger.LogInformation("CheckRunner stopped");
    }

    public async Task RunChecksAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var client = _httpClientFactory.CreateClient();

        var scheduled = await db.ScheduledChecks
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var sc in scheduled)
        {
            int status = 0;
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(RequestTimeout);

                var resp = await client.GetAsync(sc.Domain, cts.Token);
                status = (int)resp.StatusCode;
            }
            catch
            {
                status = 408;
            }

            db.Checks.Add(new Check
            {
                ScheduledCheckId = sc.Id,
                StatusCode = status,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Checked {Domain} => {Status}", sc.Domain, status);
        }

        await db.SaveChangesAsync(ct);
    }
}
