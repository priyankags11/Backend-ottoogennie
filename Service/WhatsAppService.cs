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

    // ── Booking confirmation (user + all admins) ──────────────────
    public void SendBookingConfirmation(User user, Booking booking)
    {
        var from = _config["Twilio:FromNumber"];
        if (!InitTwilio()) return;

        var addressParts = new[]
        {
            booking.AddressLine1, booking.AddressLine2,
            booking.Landmark, booking.AddressCity,
            booking.AddressState, booking.Pincode
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        var addressStr = string.Join(", ", addressParts);
        var paymentLabel = booking.PaymentMethod?.ToLower() == "upi"
            ? "UPI / Online (Zoho Billing)" : "Cash on Delivery";

        // ── User message ──
        var userMsg =
            $"✅ *Booking Confirmed — RIDE REVIVE*\n\n" +
            $"Hi *{user.Name}*, your service is booked! 🎉\n\n" +
            $"🚗 *Vehicle:* {booking.Brand?.ToUpper()} {booking.CarModel} ({booking.FuelType})\n" +
            $"🔧 *Package:* {booking.PackageName} — ₹{booking.Price:N0} ({booking.Duration})\n" +
            $"📅 *Slot:* {booking.SlotDate} at {booking.SlotTime}\n" +
            $"📍 *Address:* {addressStr}\n" +
            $"💳 *Payment:* {paymentLabel}\n" +
            $"🆔 *Booking ID:* {booking.Id}\n\n" +
            $"Our technician will arrive on time.\n" +
            $"Thank you for choosing *RIDE REVIVE* 🙌";

        Send(from!, $"+91{user.PhoneNumber}", userMsg);

        // ── Admin message ──
        var adminMsg =
            $"🔔 *New Booking — RIDE REVIVE*\n\n" +
            $"👤 *Customer:* {user.Name} · {user.PhoneNumber}\n" +
            $"✉️ *Email:* {user.Email}\n" +
            $"🏙️ *City:* {user.City}\n\n" +
            $"🚗 {booking.Brand?.ToUpper()} {booking.CarModel} ({booking.FuelType})\n" +
            $"🔧 {booking.PackageName} — ₹{booking.Price:N0}\n" +
            $"📅 {booking.SlotDate} at {booking.SlotTime}\n" +
            $"📍 {addressStr}\n" +
            $"💳 {paymentLabel}\n" +
            $"🆔 {booking.Id}";

        var admins = _context.Admins.Where(a => a.IsActive && !string.IsNullOrEmpty(a.Phone)).ToList();
        foreach (var admin in admins)
            Send(from!, $"+91{admin.Phone}", adminMsg);
    }

    // ── Review request after service Completed ────────────────────
    public void SendReviewRequest(User user, Booking booking)
    {
        var from = _config["Twilio:FromNumber"];
        var baseUrl = _config["App:FrontendUrl"];

        if (!InitTwilio()) return;

        // Build quick-rate links — clicking opens the review page pre-filled
        var rateLinks = new System.Text.StringBuilder();
        for (int i = 5; i >= 1; i--)
        {
            var stars = new string('⭐', i);
            rateLinks.AppendLine($"{stars} → {baseUrl}/review?bookingId={booking.Id}&rating={i}");
        }

        var msg =
            $"🎉 *Service Completed — RIDE REVIVE*\n\n" +
            $"Hi *{user.Name}*, your *{booking.PackageName}* service for " +
            $"{booking.Brand?.ToUpper()} {booking.CarModel} is done!\n\n" +
            $"We'd love to hear your feedback.\n" +
            $"*How would you rate our service?*\n\n" +
            rateLinks.ToString().TrimEnd() + "\n\n" +
            $"Your rating helps us improve and serve you better. 🙏\n" +
            $"Thank you for choosing *RIDE REVIVE* ⚡";

        Send(from!, $"+91{user.PhoneNumber}", msg);
        _logger.LogInformation("Review request sent to {Phone} for booking {Id}", user.PhoneNumber, booking.Id);
    }

    // ── Private helpers ───────────────────────────────────────────
    private bool InitTwilio()
    {
        var sid = _config["Twilio:AccountSid"];
        var token = _config["Twilio:AuthToken"];
        if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Twilio credentials not configured.");
            return false;
        }
        TwilioClient.Init(sid, token);
        return true;
    }

    private void Send(string from, string to, string body)
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