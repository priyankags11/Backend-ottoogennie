public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // ── User reference ──
    public Guid UserId { get; set; }
    public User? User { get; set; }

    // ── Vehicle details (from select-vehicle step) ──
    public string? FuelType { get; set; }   // Petrol / Diesel / CNG / Electric
    public string? Brand { get; set; }   // honda, tata, etc.
    public string? CarModel { get; set; }   // City, Nexon, etc.

    // ── Package details (from select-package step) ──
    public string? PackageName { get; set; }   // Basic / Standard / Comprehensive
    public decimal Price { get; set; }
    public decimal ActualPrice { get; set; }
    public string? Duration { get; set; }   // "4 hrs"

    // ── Slot (from slot-selection step) ──
    public string? SlotDate { get; set; }   // "2025-04-20"
    public string? SlotTime { get; set; }   // "10:00 AM"

    // ── Address (from address step) ──
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? Landmark { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressState { get; set; }
    public string? Pincode { get; set; }

    // ── Payment ──
    public string? PaymentMethod { get; set; }   // cash / upi

    // ── Status & audit ──
    public string Status { get; set; } = "Confirmed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}