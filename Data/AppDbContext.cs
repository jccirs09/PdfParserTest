using Microsoft.EntityFrameworkCore;

namespace PickingListApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PickingList> PickingLists { get; set; }
    public DbSet<Party> Parties { get; set; }
    public DbSet<PickingListItem> PickingListItems { get; set; }
    public DbSet<ItemTagDetail> ItemTagDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PickingList
        modelBuilder.Entity<PickingList>(entity =>
        {
            entity.HasIndex(e => e.SalesOrderNumber).IsUnique();

            entity.HasOne(e => e.SoldTo)
                .WithMany()
                .HasForeignKey(e => e.SoldToId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ShipTo)
                .WithMany()
                .HasForeignKey(e => e.ShipToId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure PickingListItem
        modelBuilder.Entity<PickingListItem>(entity =>
        {
            entity.HasOne(e => e.PickingList)
                .WithMany(p => p.Items)
                .HasForeignKey(e => e.PickingListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ItemTagDetail
        modelBuilder.Entity<ItemTagDetail>(entity =>
        {
            entity.HasOne(e => e.PickingListItem)
                .WithMany(p => p.TagDetails)
                .HasForeignKey(e => e.PickingListItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
