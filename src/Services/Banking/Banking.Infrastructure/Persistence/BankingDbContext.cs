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
            .Entries<Enterprise.BuildingBlocks.Domain.Aggregates.AggregateRoot>()
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
            .Where(e => e.Entity is Enterprise.BuildingBlocks.Domain.Entities.Entity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified))
            .ToList();

        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            var entity = (Enterprise.BuildingBlocks.Domain.Entities.Entity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
            }

            entity.UpdatedAt = now;
        }
    }
}
