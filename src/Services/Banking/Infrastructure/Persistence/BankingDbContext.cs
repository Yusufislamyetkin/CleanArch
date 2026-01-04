using Enterprise.BuildingBlocks.Domain.Aggregates;
using Enterprise.BuildingBlocks.Domain.ValueObjects;
using Enterprise.Services.Banking.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.Services.Banking.Infrastructure.Persistence;

/// <summary>
/// Banking bounded context database context
/// Configures EF Core mappings and relationships
/// </summary>
public class BankingDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<AccountActivity> AccountActivities { get; set; } = null!;

    public BankingDbContext(DbContextOptions<BankingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new AccountActivityConfiguration());

        // Global filters
        modelBuilder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<Transaction>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<AccountActivity>().HasQueryFilter(aa => !aa.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Handle domain events before saving
        await DispatchDomainEventsAsync(cancellationToken);

        // Set audit properties
        SetAuditProperties();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // Clear domain events to prevent duplicate dispatching
        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        // Dispatch events (in a real implementation, this would publish to message bus)
        foreach (var domainEvent in domainEvents)
        {
            // TODO: Publish to message bus
            // await _messageBus.PublishAsync(domainEvent, cancellationToken);
        }
    }

    private void SetAuditProperties()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Entity && (e.State == EntityState.Added || e.State == EntityState.Modified))
            .ToList();

        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            var entity = (Entity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
            }

            entity.UpdatedAt = now;
        }
    }
}

/// <summary>
/// Account entity configuration
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountNumber)
            .HasConversion(
                v => v.Value,
                v => AccountNumber.From(v))
            .HasMaxLength(34)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.Balance)
            .HasConversion(
                v => $"{v.Amount}:{v.Currency}",
                v => ParseMoney(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.MinimumBalance)
            .HasConversion(
                v => v.HasValue ? $"{v.Value.Amount}:{v.Value.Currency}" : null,
                v => string.IsNullOrEmpty(v) ? null : ParseMoney(v))
            .HasMaxLength(50);

        builder.Property(a => a.DailyTransactionLimit)
            .HasConversion(
                v => v.HasValue ? $"{v.Value.Amount}:{v.Value.Currency}" : null,
                v => string.IsNullOrEmpty(v) ? null : ParseMoney(v))
            .HasMaxLength(50);

        builder.Property(a => a.OpenedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey("AccountId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Activities)
            .WithOne()
            .HasForeignKey("AccountId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.AccountNumber).IsUnique();
        builder.HasIndex(a => a.CustomerId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => a.CreatedAt);
    }

    private static Money ParseMoney(string value)
    {
        var parts = value.Split(':');
        return Money.Of(decimal.Parse(parts[0]), parts[1]);
    }
}

/// <summary>
/// Transaction entity configuration
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasConversion(
                v => $"{v.Amount}:{v.Currency}",
                v => ParseMoney(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.ExternalReference)
            .HasMaxLength(100);

        builder.Property(t => t.Timestamp)
            .IsRequired();

        // Indexes
        builder.HasIndex(t => new { t.AccountId, t.Timestamp });
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => t.RelatedTransactionId);
    }

    private static Money ParseMoney(string value)
    {
        var parts = value.Split(':');
        return Money.Of(decimal.Parse(parts[0]), parts[1]);
    }
}

/// <summary>
/// Account activity entity configuration
/// </summary>
public class AccountActivityConfiguration : IEntityTypeConfiguration<AccountActivity>
{
    public void Configure(EntityTypeBuilder<AccountActivity> builder)
    {
        builder.ToTable("AccountActivities");

        builder.HasKey(aa => aa.Id);

        builder.Property(aa => aa.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(aa => aa.Amount)
            .HasConversion(
                v => $"{v.Amount}:{v.Currency}",
                v => ParseMoney(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(aa => aa.BalanceAfter)
            .HasConversion(
                v => $"{v.Amount}:{v.Currency}",
                v => ParseMoney(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(aa => aa.BalanceBefore)
            .HasConversion(
                v => v.HasValue ? $"{v.Value.Amount}:{v.Value.Currency}" : null,
                v => string.IsNullOrEmpty(v) ? null : ParseMoney(v))
            .HasMaxLength(50);

        builder.Property(aa => aa.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(aa => aa.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(aa => aa.Timestamp)
            .IsRequired();

        // Indexes
        builder.HasIndex(aa => new { aa.AccountId, aa.Timestamp });
        builder.HasIndex(aa => aa.Type);
    }

    private static Money ParseMoney(string value)
    {
        var parts = value.Split(':');
        return Money.Of(decimal.Parse(parts[0]), parts[1]);
    }
}
