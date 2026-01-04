using Microsoft.EntityFrameworkCore;
using WarehouseMvc.Models;

namespace WarehouseMvc.Data
{
    public class WarehouseContext : DbContext
    {
        public WarehouseContext(DbContextOptions<WarehouseContext> options)
            : base(options) { }

        public DbSet<Item> Items => Set<Item>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();

        public DbSet<Sale> Sales => Set<Sale>();

        //  NEW: users table
        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().HasIndex(i => i.Sku).IsUnique();
            modelBuilder.Entity<Item>().HasIndex(i => i.Name);

            modelBuilder.Entity<StockMovement>()
                .HasOne(m => m.Item)
                .WithMany(i => i.Movements)
                .HasForeignKey(m => m.ItemId)
                .OnDelete(DeleteBehavior.Restrict);//specifies deleting a record

            modelBuilder.Entity<StockMovement>()
                .HasOne(m => m.FromLocation)
                .WithMany()
                .HasForeignKey(m => m.FromLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(m => m.ToLocation)
                .WithMany()
                .HasForeignKey(m => m.ToLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
