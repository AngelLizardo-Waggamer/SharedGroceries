namespace BackSharedGroceries.Data
{
    using Microsoft.EntityFrameworkCore;
    using BackSharedGroceries.Models;
    using BackSharedGroceries.Enums;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Family> Families { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // "ProductStatus" enum mapping to Postgres Enum type generation
            modelBuilder.HasPostgresEnum<ProductStatus>();

            // Configure Family relationships and delete behaviors
            modelBuilder.Entity<Family>()
                .HasMany(f => f.Lists)
                .WithOne(l => l.Family)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Family>()
                .HasMany(f => f.Users)
                .WithOne(u => u.Family)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure ShoppingList relationships
            modelBuilder.Entity<ShoppingList>()
                .HasMany(sl => sl.Products)
                .WithOne(p => p.List)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Product relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.LastModifiedByUser)
                .WithMany(u => u.ModifiedProducts)
                .HasForeignKey(p => p.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserDevice relationship
            modelBuilder.Entity<UserDevice>()
                .HasOne(ud => ud.User)
                .WithOne()
                .HasForeignKey<UserDevice>(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes for performance optimization
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ListId)
                .HasDatabaseName("idx_products_list_id");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.UpdatedAt)
                .HasDatabaseName("idx_products_updated_at");

            modelBuilder.Entity<ShoppingList>()
                .HasIndex(sl => sl.FamilyId)
                .HasDatabaseName("idx_shopping_lists_family_id");

            modelBuilder.Entity<Family>()
                .HasIndex(f => f.FamilyInviteCode)
                .HasDatabaseName("idx_families_invite_code");
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Product && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                ((Product)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}