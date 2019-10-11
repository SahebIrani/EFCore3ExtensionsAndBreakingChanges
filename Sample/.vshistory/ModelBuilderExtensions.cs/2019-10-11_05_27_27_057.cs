using System;
using System.Linq;
using System.Reflection;
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
            //if (serviceProvider.GetService(typeof(IMigrator)) is IMigrator migrator)
            //{
            //    await migrator.MigrateAsync();
            //}
        }

        /// <summary>
        /// Migrate database to the latest version if supported by underlying database provider
        /// (so won't fail for InMemory database provider)
        /// </summary>
        /// <param name="databaseFacade"></param>
        public static void MigrateIfSupported(this DatabaseFacade databaseFacade)
        {
            var serviceProvider = databaseFacade.GetService<IServiceProvider>();
            //if (serviceProvider.GetService(typeof(IMigrator)) is IMigrator migrator)
            //{
            //    migrator.Migrate();
            //}
        }

        /// <summary>
        /// Configure EFCore to throw when SQL can't be generated (instead of loading everything to the client)
        /// </summary>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        public static DbContextOptionsBuilder ThrowOnQueryClientEvaluation(this DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.ConfigureWarnings(c => c.Throw(RelationalEventId.QueryClientEvaluationWarning));
            return optionsBuilder;
        }


        public static void ApplyAllConfigurationsFromCurrentAssembly(
            this ModelBuilder modelBuilder,
            Assembly assembly,
            string configNamespace = "")
        {


            // get ApplyConfiguration method with reflection
            var applyGenericMethod = typeof(ModelBuilder).GetMethod("ApplyConfiguration", BindingFlags.Instance | BindingFlags.Public);
            // replace GetExecutingAssembly with assembly where your configurations are if necessary
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters))
            {
                // use type.Namespace to filter by namespace if necessary
                foreach (var iface in type.GetInterfaces())
                {
                    // if type implements interface IEntityTypeConfiguration<SomeEntity>
                    if (iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        // make concrete ApplyConfiguration<SomeEntity> method
                        var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(iface.GenericTypeArguments[0]);
                        // and invoke that with fresh instance of your configuration type
                        applyConcreteMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(type) });
                        break;
                    }
                }
            }





            var applyGenericMethods = typeof(ModelBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var applyGenericApplyConfigurationMethods = applyGenericMethods.Where(m => m.IsGenericMethod && m.Name.Equals("ApplyConfiguration", StringComparison.OrdinalIgnoreCase));
            var applyGenericMethodd = applyGenericApplyConfigurationMethods.Where(m => m.GetParameters().FirstOrDefault().ParameterType.Name == "IEntityTypeConfiguration`1").FirstOrDefault();

            var applicableTypes = assembly
                .GetTypes()
                .Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters);

            if (!string.IsNullOrEmpty(configNamespace))
            {
                applicableTypes = applicableTypes.Where(c => c.Namespace == configNamespace);
            }

            foreach (var type in applicableTypes)
            {
                foreach (var iface in type.GetInterfaces())
                {
                    // if type implements interface IEntityTypeConfiguration<SomeEntity>
                    if (iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        // make concrete ApplyConfiguration<SomeEntity> method
                        var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(iface.GenericTypeArguments[0]);
                        // and invoke that with fresh instance of your configuration type
                        applyConcreteMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(type) });
                        Console.WriteLine("applied model " + type.Name);
                        break;
                    }
                }
            }
        }
    }
}
