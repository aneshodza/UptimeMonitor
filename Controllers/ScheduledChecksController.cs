using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UptimeMonitor.Data;
using UptimeMonitor.Models;

namespace UptimeMonitor.Controllers;

[Authorize]
public class ScheduledChecksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ScheduledChecksController> _logger;

    public ScheduledChecksController(ApplicationDbContext context,
                                     UserManager<User> userManager,
                                     ILogger<ScheduledChecksController> logger)
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
            return Challenge(); // redirect to login if not authenticated
        }

        var scheduledChecks =
            await _context.ScheduledChecks.WithUptimeAndRecentChecks(user.Id)
                .ToListAsync();
        return View(scheduledChecks);
    }

    [HttpGet]
    public IActionResult Create() { return View(); }

    [HttpPost]
    public async Task<IActionResult> Create(ScheduledCheck check)
    {
        if (!ModelState.IsValid)
        {
            return View(check);
        }

        var user = await _userManager.GetUserAsync(User);

        var existingCheck =
            await _context.ScheduledChecks.Include(sc => sc.Users)
                .FirstOrDefaultAsync(sc => sc.Domain == check.Domain);
        if (existingCheck == null)
        {
            existingCheck = new ScheduledCheck
            {
                Domain = check.Domain,
                Users = new List<User> { user! },
            };
            _context.ScheduledChecks.Add(existingCheck);
        }
        else
        {
            if (!existingCheck.Users.Any(u => u.Id == user!.Id))
            {
                existingCheck.Users.Add(user!);
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        var check = await _context.ScheduledChecks.Include(sc => sc.Users)
                        .FirstOrDefaultAsync(sc => sc.Id == id);

        if (check == null)
        {
            return NotFound();
        }

        check.Users.Remove(user!);
        if (!check.Users.Any())
        {
            _context.ScheduledChecks.Remove(check);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
                   NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ??
                                                     HttpContext.TraceIdentifier
        });
    }
}
