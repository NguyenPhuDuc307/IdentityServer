using vnLab.BackendServer.Data.Interfaces;
using vnLab.BackendServer.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace vnLab.BackendServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<EntityEntry> modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
            foreach (EntityEntry item in modified)
            {
                if (item.Entity is IDateTracking changedOrAddedItem)
                {
                    if (item.State == EntityState.Added)
                    {
                        changedOrAddedItem.CreateDate = DateTime.Now;
                    }
                    else
                    {
                        changedOrAddedItem.LastModifiedDate = DateTime.Now;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
            builder.Entity<User>().Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
            builder.Entity<Permission>()
                    .HasKey(c => new { c.RoleId, c.FunctionId, c.CommandId });
            builder.Entity<CommandInFunction>()
                    .HasKey(c => new { c.CommandId, c.FunctionId });

            builder.HasSequence("vnLab_Seq");
        }

        public DbSet<Command> Commands { set; get; } = default!;
        public DbSet<CommandInFunction> CommandInFunctions { set; get; } = default!;
        public DbSet<ActivityLog> ActivityLogs { set; get; } = default!;
        public DbSet<Function> Functions { set; get; } = default!;
        public DbSet<Permission> Permissions { set; get; } = default!;
        public DbSet<FileUpLoad> FileUpLoads { set; get; } = default!;
    }
}