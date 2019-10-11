using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Sample;

using WebApp.Models;

namespace WebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //1. Throw when can not generate a query instead of loading everything into memory
            optionsBuilder.ThrowOnQueryClientEvaluation();

            //2. Add the support for DynamicDataMasking
            //optionsBuilder.ReplaceService<IMigrationsSqlGenerator, ExtendedMigrationSqlServerGenerator>();
            //optionsBuilder.ReplaceService<IMigrationsAnnotationProvider, ExtendedSqlServerMigrationsAnnotationProvider>();

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //3. Set Cascade as a default delete behaviour
            modelBuilder.OverrideDeleteBehavior(DeleteBehavior.Cascade);

            Assembly assemblyWithConfig = Assembly.GetExecutingAssembly();
            Assembly assemblyWithConfig2 = typeof(ApplicationDbContext).Assembly;
            Assembly assemblyWithConfigurations = GetType().Assembly;
            modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
            modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfig2);
            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly(assemblyWithConfigurations);
            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly2(assemblyWithConfigurations);
            modelBuilder.ApplyAllConfigurationsFromAssembly(assemblyWithConfigurations);
            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly(assemblyWithConfig);
            modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfig);


            modelBuilder.SetGlobalMaxLength(200);
            modelBuilder.SetGlobalTablePrefix("tbl_");

            var people = this.DbSet<Person>().ToList();
        }
    }
}
