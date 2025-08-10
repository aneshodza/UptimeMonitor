using Microsoft.AspNetCore.Identity;

namespace UptimeMonitor.Models;

public class User : IdentityUser
{
  public string? Firstname { get; set; }
  public string? Lastname { get; set; }
  public List<ScheduledCheck> ScheduledChecks { get; set; } = [];
}
