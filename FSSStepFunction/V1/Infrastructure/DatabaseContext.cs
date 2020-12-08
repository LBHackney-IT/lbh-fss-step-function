using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

namespace LbhFssStepFunction.V1.Infrastructure
{

    public class DatabaseContext : DbContext
    {
        //TODO: rename DatabaseContext to reflect the data source it is representing. eg. MosaicContext.
        //Guidance on the context class can be found here https://github.com/LBHackney-IT/lbh-base-api/wiki/DatabaseContext
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<OrganisationEntity> Organisations { get; set; }
        public virtual DbSet<UserOrganisationEntity> UserOrganisations { get; set; }
        public virtual DbSet<UserEntity> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http: //go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql(
                    "Host=localhost;Database=fss-public_dev;Username=postgres;Password=mypassword");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrganisationEntity>(entity =>
            {
                entity.ToTable("organizations");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("character varying");

                entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("character varying");

                entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at");

                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<UserOrganisationEntity>(entity =>
            {
                entity.ToTable("user_organizations");

                entity.HasIndex(e => e.Id);

                entity.HasIndex(e => e.OrganisationId);

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.OrganisationId).HasColumnName("organization_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Organisation)
                    .WithMany(p => p.UserOrganisations)
                    .HasForeignKey(d => d.OrganisationId)
                    .HasConstraintName("user_organizations_organization_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserOrganisations)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_organizations_user_id_fkey");
                entity.Ignore(d => d.Organisation);
            });

            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasColumnType("character varying");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("character varying");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("character varying");

                entity.Property(e => e.SubId)
                    .HasColumnName("sub_id")
                    .HasColumnType("character varying");
                entity.Ignore(e => e.UserOrganisations);
            });
        }
    }
}
