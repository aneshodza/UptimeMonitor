namespace UptimeMonitor.Models;

public class Check 
{
  public int Id { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public int StatusCode { get; set; }

  public int ScheduledCheckId { get; set; }
  public ScheduledCheck ScheduledCheck { get; set; } = null!;
}
