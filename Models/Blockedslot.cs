public class BlockedSlot
{
    public int Id { get; set; }
    public string Date { get; set; } = "";   // "YYYY-MM-DD"
    public string SlotTime { get; set; } = "";   // "09:00 AM"
    public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}