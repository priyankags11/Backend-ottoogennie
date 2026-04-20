public class Admin
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AdminKey { get; set; } = "";   // unique 8-char key you provide
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";   // admin's WhatsApp number (91XXXXXXXXXX)
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}