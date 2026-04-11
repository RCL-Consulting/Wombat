using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<WombatIdentityUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WombatIdentityUserSpecialityScope> UserSpecialityScopes => Set<WombatIdentityUserSpecialityScope>();
    public DbSet<WombatIdentityUserSubSpecialityScope> UserSubSpecialityScopes => Set<WombatIdentityUserSubSpecialityScope>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<WombatIdentityUser>(entity =>
        {
            entity.HasMany(user => user.SpecialityScopes)
                .WithOne(scope => scope.User)
                .HasForeignKey(scope => scope.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(user => user.SubSpecialityScopes)
                .WithOne(scope => scope.User)
                .HasForeignKey(scope => scope.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WombatIdentityUserSpecialityScope>(entity =>
        {
            entity.ToTable("UserSpecialityScopes");
            entity.HasIndex(scope => new { scope.UserId, scope.SpecialityId }).IsUnique();
        });

        builder.Entity<WombatIdentityUserSubSpecialityScope>(entity =>
        {
            entity.ToTable("UserSubSpecialityScopes");
            entity.HasIndex(scope => new { scope.UserId, scope.SubSpecialityId }).IsUnique();
        });
    }
}
