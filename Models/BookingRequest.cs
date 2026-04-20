using System.ComponentModel.DataAnnotations;

public class BookingRequest
{
    // ── Step 1: User details (booking form) ──
    [Required] public string Name { get; set; } = "";
    [Required] public string PhoneNumber { get; set; } = "";
    [Required] public string Email { get; set; } = "";
    [Required] public string City { get; set; } = "";
    [Required] public string ServiceType { get; set; } = "";   // car / bike

    // ── Step 2 & 3: Vehicle (select-vehicle) ──
    public string? FuelType { get; set; }
    public string? Brand { get; set; }
    public string? CarModel { get; set; }

    // ── Step 4: Package (select-package) ──
    public string? PackageName { get; set; }
    public decimal Price { get; set; }
    public decimal ActualPrice { get; set; }
    public string? Duration { get; set; }

    // ── Step 5: Slot (slot-selection) ──
    public string? SlotDate { get; set; }
    public string? SlotTime { get; set; }

    // ── Step 6: Address ──
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? Landmark { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressState { get; set; }
    public string? Pincode { get; set; }

    // ── Step 7: Payment ──
    public string? PaymentMethod { get; set; }   // cash / upi
}