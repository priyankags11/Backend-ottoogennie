using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly WhatsAppService _whatsApp;
    private readonly ILogger<BookingController> _logger;

    public BookingController(AppDbContext context, WhatsAppService whatsApp, ILogger<BookingController> logger)
    {
        _context = context;
        _whatsApp = whatsApp;
        _logger = logger;
    }

    // ── POST /api/booking ──────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Upsert user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);

        if (user == null)
        {
            user = new User { Name = req.Name, PhoneNumber = req.PhoneNumber, Email = req.Email, City = req.City };
            _context.Users.Add(user);
        }
        else
        {
            user.Name = req.Name; user.Email = req.Email; user.City = req.City;
        }

        await _context.SaveChangesAsync();

        // 2. Save booking
        var booking = new Booking
        {
            UserId = user.Id,
            FuelType = req.FuelType,
            Brand = req.Brand,
            CarModel = req.CarModel,
            PackageName = req.PackageName,
            Price = req.Price,
            ActualPrice = req.ActualPrice,
            Duration = req.Duration,
            SlotDate = req.SlotDate,
            SlotTime = req.SlotTime,
            AddressLine1 = req.AddressLine1,
            AddressLine2 = req.AddressLine2,
            Landmark = req.Landmark,
            AddressCity = req.AddressCity,
            AddressState = req.AddressState,
            Pincode = req.Pincode,
            PaymentMethod = req.PaymentMethod,
            Status = "Confirmed"
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} created for user {UserId}", booking.Id, user.Id);

        // 3. Send WhatsApp to user + all admins (non-blocking)
        Task.Run(() => _whatsApp.SendBookingConfirmation(user, booking));

        return Ok(new
        {
            bookingId = booking.Id,
            message = "Booking Confirmed",
            service = booking.PackageName,
            carModel = booking.CarModel,
            slot = $"{booking.SlotDate} {booking.SlotTime}",
            status = booking.Status
        });
    }

    // ── GET /api/booking ──── all bookings
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
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
                b.Duration,
                b.SlotDate,
                b.SlotTime,
                b.PaymentMethod,
                user = new { b.User!.Name, b.User.PhoneNumber, b.User.Email, b.User.City }
            }).ToListAsync();

        return Ok(bookings);
    }

    // ── GET /api/booking/{id} ── single booking
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var b = await _context.Bookings.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
        return b == null ? NotFound() : Ok(b);
    }

    // ── GET /api/booking/user/{phone} ── by phone
    [HttpGet("user/{phone}")]
    public async Task<IActionResult> GetByPhone(string phone)
    {
        var bookings = await _context.Bookings.Include(b => b.User)
            .Where(b => b.User!.PhoneNumber == phone)
            .OrderByDescending(b => b.CreatedAt).ToListAsync();
        return Ok(bookings);
    }

    // ── PATCH /api/booking/{id}/status ── update status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] StatusUpdateRequest req)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        booking.Status = req.Status;
        await _context.SaveChangesAsync();
        return Ok(new { booking.Id, booking.Status });
    }
}

public class StatusUpdateRequest { public string Status { get; set; } = ""; }