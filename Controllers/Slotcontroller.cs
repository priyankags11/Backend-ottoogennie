using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class SlotController : ControllerBase
{
    private readonly AppDbContext _context;

    public SlotController(AppDbContext context)
    {
        _context = context;
    }

    // All time slots that exist in the system
    private static readonly List<string> AllMorningSlots = new()
        { "09:00 AM", "10:00 AM", "11:00 AM", "12:00 PM", "01:00 PM" };

    private static readonly List<string> AllEveningSlots = new()
        { "02:00 PM", "03:00 PM", "04:00 PM", "05:00 PM", "06:00 PM" };

    // ── GET /api/slot/available?date=2025-04-20 ───────────────────
    // Returns slots with availability computed from bookings on that date.
    // Each slot is "available" if: not blocked by admin AND
    // the number of bookings < max capacity (default 3 per slot).
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] string date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return BadRequest(new { message = "date query param required. Format: YYYY-MM-DD" });

        // Count existing confirmed bookings per slot on this date
        var bookedCounts = await _context.Bookings
            .Where(b =>
                b.SlotDate == date &&
                b.Status != "Cancelled" &&
                b.Status != "Payment Failed")
            .GroupBy(b => b.SlotTime)
            .Select(g => new { SlotTime = g.Key, Count = g.Count() })
            .ToListAsync();

        // Admin-blocked slots for this date
        var blockedSlots = await _context.BlockedSlots
            .Where(s => s.Date == date)
            .Select(s => s.SlotTime)
            .ToListAsync();

        const int maxPerSlot = 3;

        var countMap = bookedCounts.ToDictionary(x => x.SlotTime!, x => x.Count);

        SlotDto MakeSlot(string time)
        {
            var booked = countMap.TryGetValue(time, out var c) ? c : 0;
            var isBlocked = blockedSlots.Contains(time);
            return new SlotDto
            {
                Time = time,
                Available = !isBlocked && booked < maxPerSlot,
                Booked = booked,
                Blocked = isBlocked
            };
        }

        return Ok(new
        {
            date,
            morning = AllMorningSlots.Select(MakeSlot),
            evening = AllEveningSlots.Select(MakeSlot)
        });
    }

    // ── POST /api/slot/block ── Admin blocks a slot for a date ────
    [HttpPost("block")]
    public async Task<IActionResult> BlockSlot([FromBody] BlockSlotRequest req)
    {
        var existing = await _context.BlockedSlots
            .FirstOrDefaultAsync(s => s.Date == req.Date && s.SlotTime == req.SlotTime);

        if (existing != null)
            return Ok(new { message = "Already blocked" });

        _context.BlockedSlots.Add(new BlockedSlot
        {
            Date = req.Date,
            SlotTime = req.SlotTime,
            Reason = req.Reason ?? "Admin blocked"
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = $"{req.SlotTime} on {req.Date} blocked." });
    }

    // ── DELETE /api/slot/unblock ── Admin unblocks a slot ─────────
    [HttpDelete("unblock")]
    public async Task<IActionResult> UnblockSlot([FromBody] BlockSlotRequest req)
    {
        var slot = await _context.BlockedSlots
            .FirstOrDefaultAsync(s => s.Date == req.Date && s.SlotTime == req.SlotTime);

        if (slot == null) return NotFound();

        _context.BlockedSlots.Remove(slot);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Slot unblocked." });
    }

    // ── GET /api/slot/blocked?date=2025-04-20 ─────────────────────
    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlockedSlots([FromQuery] string date)
    {
        var slots = await _context.BlockedSlots
            .Where(s => s.Date == date)
            .ToListAsync();
        return Ok(slots);
    }
}

public class SlotDto
{
    public string Time { get; set; } = "";
    public bool Available { get; set; }
    public int Booked { get; set; }
    public bool Blocked { get; set; }
}

public class BlockSlotRequest
{
    public string Date { get; set; } = "";
    public string SlotTime { get; set; } = "";
    public string? Reason { get; set; }
}