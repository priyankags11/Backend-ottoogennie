using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<BlockedSlot> BlockedSlots { get; set; }  // ← new
    public DbSet<Review> Reviews { get; set; }  // ← new

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User → Bookings
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Booking → Review (one-to-one)
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Booking)
            .WithOne()
            .HasForeignKey<Review>(r => r.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Review → User
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique AdminKey
        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.AdminKey)
            .IsUnique();

        // Unique phone per user
        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();

        // Price columns
        modelBuilder.Entity<Booking>()
            .Property(b => b.Price).HasColumnType("numeric(10,2)");
        modelBuilder.Entity<Booking>()
            .Property(b => b.ActualPrice).HasColumnType("numeric(10,2)");

        // Compound index on BlockedSlot for fast lookups
        modelBuilder.Entity<BlockedSlot>()
            .HasIndex(s => new { s.Date, s.SlotTime })
            .IsUnique();

        // Seed 5 admin keys
        modelBuilder.Entity<Admin>().HasData(
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), AdminKey = "RRADM001", Name = "Admin One", Phone = "9000000001", IsActive = true },
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), AdminKey = "RRADM002", Name = "Admin Two", Phone = "9000000002", IsActive = true },
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000003"), AdminKey = "RRADM003", Name = "Admin Three", Phone = "9000000003", IsActive = true },
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000004"), AdminKey = "RRADM004", Name = "Admin Four", Phone = "9000000004", IsActive = true },
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000005"), AdminKey = "RRADM005", Name = "Admin Five", Phone = "9000000005", IsActive = true }
        );
    }
}