using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UptimeMonitor.Data;
using UptimeMonitor.Models;

namespace UptimeMonitor.Controllers;

public class HomeController : Controller
{
  private readonly ApplicationDbContext _context;
  private readonly UserManager<User> _userManager;
  private readonly ILogger<HomeController> _logger;

  public HomeController(
      ApplicationDbContext context,
      UserManager<User> userManager,
      ILogger<HomeController> logger)
  {
    _context = context;
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<IActionResult> Index()
  {
    var user = await _userManager.GetUserAsync(User);

    if (user == null)
    {
      return View(new List<UptimeMonitor.Models.ScheduledCheck>());
    }

    var scheduledChecks = await _context.ScheduledChecks
      .Where(sc => sc.Users.Any(u => u.Id == user.Id))
      .Include(sc => sc.Checks
          .OrderByDescending(c => c.CreatedAt)
          .Take(10))
      .Include(sc => sc.Users)
      .ToListAsync();
    return View(scheduledChecks);
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}
