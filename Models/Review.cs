public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }   // 1–5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Booking? Booking { get; set; }
    public User? User { get; set; }
}