using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly WhatsAppService _whatsAppService;

    public BookingController(AppDbContext context, WhatsAppService whatsAppService)
    {
        _context = context;
        _whatsAppService = whatsAppService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingRequest request)
    {
        // 1. Check user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

        // 2. Create user if not exists
        if (user == null)
        {
            user = new User
            {
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                City = request.City
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // 3. Create booking
        var booking = new Booking
        {
            UserId = user.Id,
            ServiceType = request.ServiceType
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // 4. Send WhatsApp Message ✅
        var message = $"🚗 Hello {user.Name},\n\n" +
                      $"Your *{booking.ServiceType}* booking is confirmed!\n" +
                      $"📍 Location: {user.City}\n\n" +
                      $"Our team will contact you shortly.\n\n" +
                      $"Thank you for choosing RIDE REVIVE 🙌";

        _whatsAppService.SendMessage(user.PhoneNumber, message);

        return Ok(new
        {
            message = "Booking Confirmed",
            service = booking.ServiceType
        });
    }
}