using System.Reflection;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Sample;

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
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Assembly assemblyWithConfig = Assembly.GetExecutingAssembly();
            Assembly assemblyWithConfigurations = GetType().Assembly; //get whatever assembly you want
            modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly(/*"MyRoot.Api.Entities.Configuration"*/assemblyWithConfigurations);
            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly2(/*"MyRoot.Api.Entities.Configuration"*/assemblyWithConfigurations);
            modelBuilder.ApplyAllConfigurationsFromAssembly(assemblyWithConfigurations);



        }
    }
}
