using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ranger.Common;

namespace Ranger.Services.Integrations.Data
{
    public class IntegrationsDbContext : DbContext, IDataProtectionKeyContext
    {

        public delegate IntegrationsDbContext Factory(DbContextOptions<IntegrationsDbContext> options);

        private readonly IDataProtectionProvider dataProtectionProvider;
        public IntegrationsDbContext(DbContextOptions<IntegrationsDbContext> options, IDataProtectionProvider dataProtectionProvider = null) : base(options)
        {
            this.dataProtectionProvider = dataProtectionProvider;
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<IntegrationStream> IntegrationStreams { get; set; }
        public DbSet<IntegrationUniqueConstraint> IntegrationUniqueConstraints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            EncryptingDbHelper encryptionHelper = null;
            if (dataProtectionProvider != null)
            {
                encryptionHelper = new EncryptingDbHelper(this.dataProtectionProvider);
            }

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Remove 'AspNet' prefix and convert table name from PascalCase to snake_case. E.g. AspNetRoleClaims -> role_claims
                entity.SetTableName(entity.GetTableName().Replace("AspNet", "").ToSnakeCase());

                // Convert column names from PascalCase to snake_case.
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToSnakeCase());
                }

                // Convert primary key names from PascalCase to snake_case. E.g. PK_users -> pk_users
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToSnakeCase());
                }

                // Convert foreign key names from PascalCase to snake_case.
                foreach (var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
                }

                // Convert index names from PascalCase to snake_case.
                foreach (var index in entity.GetIndexes())
                {
                    index.SetName(index.GetName().ToSnakeCase());
                }

                encryptionHelper?.SetEncrytedPropertyAccessMode(entity);
            }

            modelBuilder.Entity<IntegrationUniqueConstraint>().HasIndex(_ => new { _.DatabaseUsername, _.ProjectId }).IsUnique();
            modelBuilder.Entity<IntegrationUniqueConstraint>().HasIndex(_ => new { _.ProjectId, _.Name }).IsUnique();
        }
    }
}