using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class WhatsAppService
{
    private readonly IConfiguration _config;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly AppDbContext _context;

    public WhatsAppService(IConfiguration config, ILogger<WhatsAppService> logger, AppDbContext context)
    {
        _config = config;
        _logger = logger;
        _context = context;
    }

    public void SendBookingConfirmation(User user, Booking booking)
    {
        var sid = _config["Twilio:AccountSid"];
        var token = _config["Twilio:AuthToken"];
        var from = _config["Twilio:FromNumber"];

        if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Twilio credentials not configured. Skipping WhatsApp messages.");
            return;
        }

        try { TwilioClient.Init(sid, token); }
        catch (Exception ex) { _logger.LogError(ex, "Twilio init failed"); return; }

        var addressParts = new[]
        {
            booking.AddressLine1, booking.AddressLine2,
            booking.Landmark, booking.AddressCity,
            booking.AddressState, booking.Pincode
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        var addressStr = string.Join(", ", addressParts);
        var paymentLabel = booking.PaymentMethod?.ToLower() == "upi"
            ? "UPI / Online (Zoho Billing)" : "Cash on Delivery";

        // ── 1. USER confirmation message ──────────────────────────
        var userMsg =
            $"✅ *Booking Confirmed — RIDE REVIVE*\n\n" +
            $"Hi *{user.Name}*, your service is booked! 🎉\n\n" +
            $"🚗 *Vehicle:* {booking.Brand?.ToUpper()} {booking.CarModel} ({booking.FuelType})\n" +
            $"🔧 *Package:* {booking.PackageName} — ₹{booking.Price:N0} ({booking.Duration})\n" +
            $"📅 *Slot:* {booking.SlotDate} at {booking.SlotTime}\n" +
            $"📍 *Address:* {addressStr}\n" +
            $"💳 *Payment:* {paymentLabel}\n" +
            $"🆔 *Booking ID:* {booking.Id}\n\n" +
            $"Our technician will arrive on time. For support, reply to this message.\n\n" +
            $"Thank you for choosing *RIDE REVIVE* 🙌";

        SendWhatsApp(from!, $"+91{user.PhoneNumber}", userMsg);

        // ── 2. ADMIN alert messages — sent to ALL active admins ───
        var adminMsg =
            $"🔔 *New Booking Alert — RIDE REVIVE*\n\n" +
            $"👤 *Customer:* {user.Name}\n" +
            $"📞 *Phone:* {user.PhoneNumber}\n" +
            $"✉️ *Email:* {user.Email}\n" +
            $"🏙️ *City:* {user.City}\n\n" +
            $"🚗 *Vehicle:* {booking.Brand?.ToUpper()} {booking.CarModel} ({booking.FuelType})\n" +
            $"🔧 *Package:* {booking.PackageName} — ₹{booking.Price:N0}\n" +
            $"📅 *Slot:* {booking.SlotDate} at {booking.SlotTime}\n" +
            $"📍 *Address:* {addressStr}\n" +
            $"💳 *Payment:* {paymentLabel}\n" +
            $"🆔 *Booking ID:* {booking.Id}\n" +
            $"🕐 *Booked At:* {booking.CreatedAt:dd MMM yyyy, hh:mm tt} UTC";

        var activeAdmins = _context.Admins
            .Where(a => a.IsActive && !string.IsNullOrEmpty(a.Phone))
            .ToList();

        foreach (var admin in activeAdmins)
        {
            SendWhatsApp(from!, $"+91{admin.Phone}", adminMsg);
        }
    }

    private void SendWhatsApp(string from, string to, string body)
    {
        try
        {
            MessageResource.Create(
                body: body,
                from: new PhoneNumber($"whatsapp:{from}"),
                to: new PhoneNumber($"whatsapp:{to}")
            );
            _logger.LogInformation("WhatsApp sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhatsApp failed to {To}", to);
        }
    }
}