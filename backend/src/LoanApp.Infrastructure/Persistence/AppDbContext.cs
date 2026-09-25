using LoanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LoanApplication> Applications => Set<LoanApplication>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Ssn).IsUnique();
            entity.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(200).IsRequired();
            entity.Property(x => x.State).HasMaxLength(2).IsRequired();
            entity.Property(x => x.CompanyName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Ssn).HasMaxLength(9).IsRequired();
            entity.HasOne(x => x.Application)
                .WithOne(x => x.Customer)
                .HasForeignKey<LoanApplication>(x => x.CustomerId);
        });

        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CustomerId).IsUnique();
            entity.Property(x => x.RequestedAmount).HasColumnType("TEXT");
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
            entity.HasIndex(x => x.ProcessedAtUtc);
        });
    }
}
