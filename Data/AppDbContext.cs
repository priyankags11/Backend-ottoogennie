using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User → Bookings (one-to-many)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique phone per user
        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();

        // Unique AdminKey
        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.AdminKey)
            .IsUnique();

        // Price columns
        modelBuilder.Entity<Booking>()
            .Property(b => b.Price)
            .HasColumnType("numeric(10,2)");
        modelBuilder.Entity<Booking>()
            .Property(b => b.ActualPrice)
            .HasColumnType("numeric(10,2)");

        // ── Seed 5 admin keys ──────────────────────────────────────
        // Replace Name & Phone with your real admin details.
        // AdminKey is the unique ID you will give each admin.
        modelBuilder.Entity<Admin>().HasData(
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), AdminKey = "RRADM001", Name = "Admin One", Phone = "9000000001", IsActive = true },
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), AdminKey = "RRADM002", Name = "Admin Two", Phone = "9000000002", IsActive = true },
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000003"), AdminKey = "RRADM003", Name = "Admin Three", Phone = "9000000003", IsActive = true },
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000004"), AdminKey = "RRADM004", Name = "Admin Four", Phone = "9000000004", IsActive = true },
            new Admin { Id = Guid.Parse("11111111-0000-0000-0000-000000000005"), AdminKey = "RRADM005", Name = "Admin Five", Phone = "9000000005", IsActive = true }
        );
    }
}