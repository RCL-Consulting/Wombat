using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Wombat.Common.Models;
using Wombat.Data.Configurations.Entities;

namespace Wombat.Data
{
    public class ApplicationDbContext : IdentityDbContext<WombatUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new InstitutionConfiguration());
            builder.ApplyConfiguration(new RoleSeedConfiguration());
            builder.ApplyConfiguration(new UserSeedConfiguration());
            builder.ApplyConfiguration(new UserRoleSeedConfiguration());
            builder.ApplyConfiguration(new OptionConfiguration());
            builder.ApplyConfiguration(new OptionSetConfiguration());
            builder.ApplyConfiguration(new SpecialityConfiguration());
            builder.ApplyConfiguration(new OptionCriterionConfiguration());
            builder.ApplyConfiguration(new AssessmentFormConfiguration());//
            
            //builder.Entity<EPAForm>()
            //.HasKey(sc => new { sc.EPAId, sc.FormId });

            //builder.Entity<EPAForm>()
            //    .HasOne(sc => sc.EPA)
            //    .WithMany(s => s.Forms)
            //    .HasForeignKey(sc => sc.EPAId);

            //builder.Entity<EPAForm>()
            //    .HasOne(sc => sc.Form)
            //    .WithMany(c => c.EPAs)
            //    .HasForeignKey(sc => sc.FormId);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in base.ChangeTracker.Entries<BaseEntity>().Where(q => q.State==EntityState.Added ||
                                                                                 q.State==EntityState.Modified))
            {
                entry.Entity.DateModified = DateTime.Now;
                if (entry.State == EntityState.Added)
                    entry.Entity.DateCreated = DateTime.Now;
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<Institution> Institutions { get; set; }
        public DbSet<AssessmentForm> AssessmentForms { get; set; }
        public DbSet<EPA> EPAs { get; set; }
        public DbSet<EPAForm> EPAForms { get; set; }

        public DbSet<OptionCriterion> OptionCriteria{ get; set; }
        public DbSet<OptionSet> OptionSets{ get; set; }
        public DbSet<Option> Options { get; set; }

        public DbSet<Speciality> Specialities { get; set; }
        public DbSet<SubSpeciality> SubSpecialities { get; set; }

        public DbSet<LoggedAssessment> LoggedAssessments { get; set; }
        public DbSet<OptionCriterionResponse> OptionCriterionResponses { get; set; }
    }
}
