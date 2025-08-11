using System.ComponentModel.DataAnnotations.Schema;

namespace UptimeMonitor.Models;

public class ScheduledCheck
{
  public int Id { get; set; }

  private string? _domain;
  public string? Domain {
    get => _domain;
    set => _domain = NormalizeDomain(value);
  }
  public List<User> Users { get; set; } = [];
  public List<Check> Checks { get; set; } = [];

  [NotMapped]
  public double UptimePercentage { get; set; }

  private static string? NormalizeDomain(string? domain)
  {
    if (string.IsNullOrWhiteSpace(domain))
      return null;

    if (!domain.StartsWith("http://") && !domain.StartsWith("https://"))
    {
      domain = "https://" + domain;
    }

    return domain.TrimEnd('/');
  }
}
