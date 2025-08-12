namespace UptimeMonitor.Models;

public class Group 
{
  public int Id { get; set; }
  public string GroupCode { get; set; } = null!;
  public string GroupName { get; set; } = null!;

  public string OwnerId { get; set; } = null!;
  public User Owner { get; set; } = null!;

  public List<User> Members { get; set; } = [];
}
