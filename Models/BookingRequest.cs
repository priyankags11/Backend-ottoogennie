public class BookingRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();   // 🔥 REQUIRED

    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? ServiceType { get; set; }
}