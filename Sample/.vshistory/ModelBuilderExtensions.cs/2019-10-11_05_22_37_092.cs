using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Sample
{
    public static class ModelBuilderExtensions
    {
        public static void OverrideDeleteBehaviour(
            this ModelBuilder modelBuilder,
            DeleteBehavior deleteBehaviour = DeleteBehavior.Restrict)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = deleteBehaviour;
            }
        }

        /// <summary>
        /// Migrate database to the latest version if supported by underlying database provider
        /// (so won't fail for InMemory database provider)
        /// </summary>
        /// <param name="databaseFacade"></param>
        public static async Task MigrateIfSupportedAsync(this DatabaseFacade databaseFacade)
        {
            var serviceProvider = databaseFacade.GetService<IServiceProvider>();
            if (serviceProvider.GetService(typeof(IMigrator)) is IMigrator migrator)
            {
                await migrator.MigrateAsync();
            }
        }

        /// <summary>
        /// Migrate database to the latest version if supported by underlying database provider
        /// (so won't fail for InMemory database provider)
        /// </summary>
        /// <param name="databaseFacade"></param>
        public static void MigrateIfSupported(this DatabaseFacade databaseFacade)
        {
            var serviceProvider = databaseFacade.GetService<IServiceProvider>();
            if (serviceProvider.GetService(typeof(IMigrator)) is IMigrator migrator)
            {
                migrator.Migrate();
            }
        }

        /// <summary>
        /// Configure EFCore to throw when SQL can't be generated (instead of loading everything to the client)
        /// </summary>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        public static DbContextOptionsBuilder ThrowOnQueryClientEvaluation(this DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(c => c.Throw(RelationalEventId.QueryClientEvaluationWarning));
            return optionsBuilder;
        }
    }
}
