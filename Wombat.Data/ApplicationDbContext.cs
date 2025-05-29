/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

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
            builder.ApplyConfiguration(new UserSeedConfiguration());
            builder.ApplyConfiguration(new RoleSeedConfiguration());
            builder.ApplyConfiguration(new UserRoleSeedConfiguration());
            builder.ApplyConfiguration(new OptionConfiguration());
            builder.ApplyConfiguration(new OptionSetConfiguration());
            builder.ApplyConfiguration(new SpecialityConfiguration());
            builder.ApplyConfiguration(new SubSpecialityConfiguration());
            builder.ApplyConfiguration(new OptionCriterionConfiguration());
            builder.ApplyConfiguration(new AssessmentFormConfiguration());
            builder.ApplyConfiguration(new EPAConfiguration());
            builder.ApplyConfiguration(new EPAFormConfiguration());
            builder.ApplyConfiguration(new EPACurriculumConfiguration());

            builder.Entity<RegistrationInvitation>(entity =>
            {
                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(e => e.Token)
                      .IsRequired();

                entity.Property(e => e.Roles)
                      .IsRequired();
            });

            builder.Entity<AssessmentRequest>()
                .HasOne(ar => ar.LoggedAssessment)
                .WithOne(la => la.AssessmentRequest)
                .HasForeignKey<LoggedAssessment>(la => la.AssessmentRequestId);
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
        public DbSet<AssessmentRequest> AssessmentRequests { get; set; }

        public DbSet<EPA> EPAs { get; set; }
        public DbSet<EPAForm> EPAForms { get; set; }
        public DbSet<EPACurriculum> EPACurricula { get; set; }

        public DbSet<OptionCriterion> OptionCriteria{ get; set; }
        public DbSet<OptionSet> OptionSets{ get; set; }
        public DbSet<Option> Options { get; set; }

        public DbSet<Speciality> Specialities { get; set; }
        public DbSet<SubSpeciality> SubSpecialities { get; set; }

        public DbSet<LoggedAssessment> LoggedAssessments { get; set; }
        public DbSet<OptionCriterionResponse> OptionCriterionResponses { get; set; }

        public DbSet<RegistrationInvitation> RegistrationInvitations { get; set; }
    }
}
