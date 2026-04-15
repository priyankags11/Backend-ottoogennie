
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();


    public string? Name { get; set; }


    public string? PhoneNumber { get; set; }

    public string? City { get; set; }
}