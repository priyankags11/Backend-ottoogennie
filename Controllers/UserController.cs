using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/user/{phone}
    [HttpGet("{phone}")]
    public async Task<IActionResult> GetByPhone(string phone)
    {
        var user = await _context.Users
            .Include(u => u.Bookings)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (user == null) return NotFound();
        return Ok(user);
    }

    // GET /api/user
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users
            .Include(u => u.Bookings)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return Ok(users);
    }
}