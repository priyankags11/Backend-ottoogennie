using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ── POST /api/auth/user ────────────────────────────────────────
    // User login: match by phone OR email
    [HttpPost("user")]
    public async Task<IActionResult> UserLogin([FromBody] UserLoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PhoneOrEmail))
            return BadRequest(new { message = "Phone number or email is required." });

        var input = req.PhoneOrEmail.Trim().ToLower();

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            (u.PhoneNumber != null && u.PhoneNumber.ToLower() == input) ||
            (u.Email != null && u.Email.ToLower() == input)
        );

        if (user == null)
            return NotFound(new { message = "No account found with this phone number or email." });

        var bookings = await _context.Bookings
            .Where(b => b.UserId == user.Id)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.Id,
                b.Status,
                b.CreatedAt,
                b.FuelType,
                b.Brand,
                b.CarModel,
                b.PackageName,
                b.Price,
                b.Duration,
                b.SlotDate,
                b.SlotTime,
                b.AddressLine1,
                b.AddressCity,
                b.AddressState,
                b.Pincode,
                b.PaymentMethod
            })
            .ToListAsync();

        return Ok(new
        {
            role = "user",
            userId = user.Id,
            name = user.Name,
            phone = user.PhoneNumber,
            email = user.Email,
            city = user.City,
            bookings
        });
    }

    // ── POST /api/auth/admin ───────────────────────────────────────
    // Admin login: match by unique AdminKey
    [HttpPost("admin")]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.AdminKey))
            return BadRequest(new { message = "Admin key is required." });

        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.AdminKey == req.AdminKey.Trim() && a.IsActive);

        if (admin == null)
            return Unauthorized(new { message = "Invalid or inactive admin key." });

        // Return all bookings for admin view
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.Id,
                b.Status,
                b.CreatedAt,
                b.FuelType,
                b.Brand,
                b.CarModel,
                b.PackageName,
                b.Price,
                b.ActualPrice,
                b.Duration,
                b.SlotDate,
                b.SlotTime,
                b.AddressLine1,
                b.AddressLine2,
                b.Landmark,
                b.AddressCity,
                b.AddressState,
                b.Pincode,
                b.PaymentMethod,
                customer = new
                {
                    b.User!.Name,
                    b.User.PhoneNumber,
                    b.User.Email,
                    b.User.City
                }
            })
            .ToListAsync();

        _logger.LogInformation("Admin {Name} logged in", admin.Name);

        return Ok(new
        {
            role = "admin",
            adminId = admin.Id,
            name = admin.Name,
            bookings,
            total = bookings.Count
        });
    }
}

// ── Request DTOs ──────────────────────────────────────────────────
public class UserLoginRequest
{
    public string PhoneOrEmail { get; set; } = "";
}

public class AdminLoginRequest
{
    public string AdminKey { get; set; } = "";
}