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
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
        if (user == null)
        {
            user = new User { Name = req.Name, PhoneNumber = req.PhoneNumber, Email = req.Email, City = req.City };
            _context.Users.Add(user);
        }
        else { user.Name = req.Name; user.Email = req.Email; user.City = req.City; }

        await _context.SaveChangesAsync();

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

        await Task.Run(() => _whatsApp.SendBookingConfirmation(user, booking));

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

    // ── GET /api/booking ──────────────────────────────────────────
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
                b.AddressLine1,
                b.AddressCity,
                b.AddressState,
                b.Pincode,
                user = new { b.User!.Name, b.User.PhoneNumber, b.User.Email, b.User.City },
                // Include review if exists
                review = _context.Reviews
                    .Where(r => r.BookingId == b.Id)
                    .Select(r => new { r.Rating, r.Comment, r.CreatedAt })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(bookings);
    }

    // ── GET /api/booking/{id} ──────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var b = await _context.Bookings.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
        return b == null ? NotFound() : Ok(b);
    }

    // ── GET /api/booking/user/{phone} ─────────────────────────────
    [HttpGet("user/{phone}")]
    public async Task<IActionResult> GetByPhone(string phone)
    {
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Where(b => b.User!.PhoneNumber == phone)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return Ok(bookings);
    }

    // ── PATCH /api/booking/{id}/status ── FIXED ───────────────────
    // Bug was: status string mismatch.
    // Fix: exact match, send review WhatsApp when status → Completed.
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] StatusUpdateRequest req)
    {
        // Normalise to exact expected values to prevent case/space issues
        var validStatuses = new[] { "Confirmed", "In Progress", "Completed", "Cancelled" };
        var normalised = validStatuses.FirstOrDefault(s =>
            string.Equals(s, req.Status.Trim(), StringComparison.OrdinalIgnoreCase));

        if (normalised == null)
            return BadRequest(new { message = $"Invalid status. Valid: {string.Join(", ", validStatuses)}" });

        var booking = await _context.Bookings
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        var previousStatus = booking.Status;
        booking.Status = normalised;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Booking {Id} status: {Old} → {New}", id, previousStatus, normalised);

        // Send WhatsApp review request when service is marked Completed
        if (normalised == "Completed" && previousStatus != "Completed" && booking.User != null)
        {
            await Task.Run(() => _whatsApp.SendReviewRequest(booking.User, booking));
        }

        return Ok(new { booking.Id, booking.Status });
    }

    // ── POST /api/booking/{id}/review ─────────────────────────────
    // Customer submits rating via WhatsApp link → this endpoint stores it.
    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> SubmitReview(Guid id, [FromBody] ReviewRequest req)
    {
        if (req.Rating < 1 || req.Rating > 5)
            return BadRequest(new { message = "Rating must be between 1 and 5." });

        var booking = await _context.Bookings
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound(new { message = "Booking not found." });
        if (booking.Status != "Completed")
            return BadRequest(new { message = "Can only review completed bookings." });

        // Prevent duplicate reviews
        var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.BookingId == id);
        if (existing != null)
        {
            existing.Rating = req.Rating;
            existing.Comment = req.Comment;
        }
        else
        {
            _context.Reviews.Add(new Review
            {
                BookingId = id,
                UserId = booking.UserId,
                Rating = req.Rating,
                Comment = req.Comment
            });
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Review submitted for booking {Id}: {Rating}★", id, req.Rating);
        return Ok(new { message = "Thank you for your review!", rating = req.Rating });
    }

    // ── GET /api/booking/{id}/review ──────────────────────────────
    [HttpGet("{id:guid}/review")]
    public async Task<IActionResult> GetReview(Guid id)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.BookingId == id);
        return review == null ? NotFound() : Ok(review);
    }
}

public class StatusUpdateRequest { public string Status { get; set; } = ""; }

public class ReviewRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}