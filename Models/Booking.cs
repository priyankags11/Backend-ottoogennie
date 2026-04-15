using System.ComponentModel.DataAnnotations.Schema;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();


    public Guid UserId { get; set; }

    public string? ServiceType { get; set; }

    public string Status { get; set; } = "Confirmed";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}