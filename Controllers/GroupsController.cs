using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UptimeMonitor.Data;
using UptimeMonitor.Models;

namespace UptimeMonitor.Controllers;

[Authorize]
public class GroupsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ScheduledChecksController> _logger;

    public GroupsController(ApplicationDbContext context,
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

        var groups =
            await _context.Groups.Where(g => g.Members.Any(m => m.Id == user.Id))
                .Include(g => g.Members)
                .ToListAsync();
        return View(groups);
    }

    public async Task<IActionResult> Show(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        var group = await _context.Groups.AsNoTracking()
                        .Include(g => g.Owner)
                        .Include(g => g.Members)
                        .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound();

        var isOwnerOrMember =
            user != null &&
            (group.OwnerId == user.Id || group.Members.Any(m => m.Id == user.Id));
        if (!isOwnerOrMember)
            return Forbid();

        return View(group);
    }

    [HttpGet]
    public IActionResult Create() { return View(); }

    [HttpPost]
    public async Task<IActionResult> Create(Group group)
    {
        var user = await _userManager.GetUserAsync(User);

        ModelState.Remove(nameof(Group.GroupCode));
        ModelState.Remove(nameof(Group.OwnerId));
        ModelState.Remove(nameof(Group.Owner));

        group.OwnerId = user!.Id;
        group.Members.Add(user);

        if (!ModelState.IsValid)
        {
            return View(group);
        }

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Join() { return View(); }

    [HttpPost]
    public async Task<IActionResult> Join(Group group)
    {
        var user = await _userManager.GetUserAsync(User);

        var foundGroup =
            await _context.Groups.Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.GroupCode == group.GroupCode);

        if (foundGroup == null)
        {
            ModelState.AddModelError(nameof(Group.GroupCode), "Cannot find group");
            return View(group);
        }

        var alreadyMember = await _context.Entry(foundGroup)
                                .Collection(g => g.Members)
                                .Query()
                                .AnyAsync(m => m.Id == user!.Id);

        if (!alreadyMember)
        {
            // Reuse the same user instance; attach if needed
            if (_context.Entry(user).State == EntityState.Detached)
                _context.Attach(user);

            foundGroup.Members.Add(user);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Show), new { id = foundGroup.Id });
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
