using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    // CREATE USER
    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        user.Id = Guid.NewGuid();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(user);
    }

    // GET USER BY PHONE
    [HttpGet("{phone}")]
    public IActionResult GetByPhone(string phone)
    {
        var user = _context.Users.FirstOrDefault(x => x.PhoneNumber == phone);
        return Ok(user);
    }
}