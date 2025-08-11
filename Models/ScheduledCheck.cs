namespace UptimeMonitor.Models;

public class ScheduledCheck
{
  public int Id { get; set; }
  public string? Domain { get; set; }
  public List<User> Users { get; set; } = [];
  public List<Check> Checks { get; set; } = [];
}
