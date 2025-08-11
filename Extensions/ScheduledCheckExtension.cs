using UptimeMonitor.Models;

public static class ScheduledCheckExtensions
{
    public static IQueryable<ScheduledCheck> WithUptimeAndRecentChecks(
        this IQueryable<ScheduledCheck> query,
        string userId)
    {
        return query
            .Where(sc => sc.Users.Any(u => u.Id == userId))
            .Select(sc => new ScheduledCheck
            {
                Id = sc.Id,
                Domain = sc.Domain,
                Checks = sc.Checks
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .ToList(),
                Users = sc.Users,
                UptimePercentage = sc.Checks
                    .Select(c => c.StatusCode / 100)
                    .GroupBy(_ => 1)
                    .Select(g => (g.Count(x => x >= 1 && x < 4) * 1.0) /
                                 (g.Count() * 1.0) * 100)
                    .FirstOrDefault()
            });
    }
}
