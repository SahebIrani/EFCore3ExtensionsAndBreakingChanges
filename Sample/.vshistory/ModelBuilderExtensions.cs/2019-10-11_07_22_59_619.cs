using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Sample
{
    public static class ModelBuilderExtensions
    {
        public static void OverrideDeleteBehavior(
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

        //
        // Summary:
        //     /// Applies configuration from all Microsoft.EntityFrameworkCore.IEntityTypeConfiguration`1
        //     and Microsoft.EntityFrameworkCore.IQueryTypeConfiguration`1 /// instances that
        //     are defined in provided assembly. ///
        //
        // Parameters:
        //   assembly:
        //     The assembly to scan.
        //
        //   predicate:
        //     Optional predicate to filter types within the assembly.
        //
        // Returns:
        //     /// The same Microsoft.EntityFrameworkCore.ModelBuilder instance so that additional
        //     configuration calls can be chained. ///
        //public static ModelBuilder ApplyConfigurationsFromAssembly(ModelBuilder modelBuilder, Assembly assembly, Func<Type, bool> predicate = null)
        //{
        //    MethodInfo methodInfo = typeof(ModelBuilder).GetMethods().Single(delegate (MethodInfo e)
        //    {
        //        if (e.Name == "ApplyConfiguration" && e.ContainsGenericParameters)
        //        {
        //            return e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>);
        //        }
        //        return false;
        //    });
        //    MethodInfo methodInfo2 = typeof(ModelBuilder).GetMethods().Single(delegate (MethodInfo e)
        //    {
        //        if (e.Name == "ApplyConfiguration" && e.ContainsGenericParameters)
        //        {
        //            return e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition() == typeof(IQueryTypeConfiguration<>);
        //        }
        //        return false;
        //    });
        //    foreach (TypeInfo constructibleType in assembly.GetConstructibleTypes())
        //    {
        //        if (!(constructibleType.GetConstructor(Type.EmptyTypes) == null) && (predicate == null || predicate(constructibleType)))
        //        {
        //            Type[] interfaces = constructibleType.GetInterfaces();
        //            foreach (Type type in interfaces)
        //            {
        //                if (type.IsGenericType)
        //                {
        //                    if (type.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
        //                    {
        //                        methodInfo.MakeGenericMethod(type.GenericTypeArguments[0]).Invoke(modelBuilder, new object[1]
        //                        {
        //                            Activator.CreateInstance(constructibleType)
        //                        });
        //                    }
        //                    else if (type.GetGenericTypeDefinition() == typeof(IQueryTypeConfiguration<>))
        //                    {
        //                        methodInfo2.MakeGenericMethod(type.GenericTypeArguments[0]).Invoke(modelBuilder, new object[1]
        //                        {
        //                            Activator.CreateInstance(constructibleType)
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return modelBuilder;
        //}

        public static void ApplyConfigurationsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly)
        {
            var configurations = assembly.DefinedTypes.Where(t =>
                 t.ImplementedInterfaces.Any(i =>
                    i.IsGenericType &&
                    i.Name.Equals(typeof(IEntityTypeConfiguration<>).Name,
                           StringComparison.InvariantCultureIgnoreCase)
                  ) &&
                  t.IsClass &&
                  !t.IsAbstract &&
                  !t.IsNested)
                  .ToList();

            foreach (var configuration in configurations)
            {
                var entityType = configuration.BaseType.GenericTypeArguments.SingleOrDefault(t => t.IsClass);

                var applyConfigMethod = typeof(ModelBuilder).GetMethod("ApplyConfiguration");

                var applyConfigGenericMethod = applyConfigMethod.MakeGenericMethod(entityType);

                applyConfigGenericMethod.Invoke(modelBuilder,
                        new object[] { Activator.CreateInstance(configuration) });
            }
        }

        public static ModelBuilder ApplyAllConfigurationsFromAssembly(
            this ModelBuilder modelBuilder,
            Assembly assembly)
        {
            var applyGenericMethod = typeof(ModelBuilder)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(m => m.Name == nameof(ModelBuilder.ApplyConfiguration)
                    && m.GetParameters().Count() == 1
                    && m.GetParameters().Single().ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));

            foreach (var type in assembly.GetTypes()
                .Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters))
            {
                foreach (var iface in type.GetInterfaces())
                {
                    if (iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(iface.GenericTypeArguments[0]);
                        applyConcreteMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(type) });
                        break;
                    }
                }
            }

            return modelBuilder;
        }

        public static void ApplyAllConfigurationsFromCurrentAssembly(this ModelBuilder modelBuilder, Assembly assembly, string configNamespace = "")
        {
            var applyGenericMethods = typeof(ModelBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var applyGenericApplyConfigurationMethods = applyGenericMethods.Where(m => m.IsGenericMethod && m.Name.Equals("ApplyConfiguration", StringComparison.OrdinalIgnoreCase));
            var applyGenericMethod = applyGenericApplyConfigurationMethods.Where(m => m.GetParameters().FirstOrDefault().ParameterType.Name == "IEntityTypeConfiguration`1").FirstOrDefault();


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


            //// get ApplyConfiguration method with reflection
            //var applyGenericMethod = typeof(ModelBuilder).GetMethod("ApplyConfiguration", BindingFlags.Instance | BindingFlags.Public);
            //// replace GetExecutingAssembly with assembly where your configurations are if necessary
            //foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
            //    .Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters))
            //{
            //    // use type.Namespace to filter by namespace if necessary
            //    foreach (var iface in type.GetInterfaces())
            //    {
            //        // if type implements interface IEntityTypeConfiguration<SomeEntity>
            //        if (iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
            //        {
            //            // make concrete ApplyConfiguration<SomeEntity> method
            //            var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(iface.GenericTypeArguments[0]);
            //            // and invoke that with fresh instance of your configuration type
            //            applyConcreteMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(type) });
            //            break;
            //        }
            //    }
            //}
        }

        public static void ApplyAllConfigurationsFromCurrentAssembly(
            this ModelBuilder modelBuilder,
            Assembly assembly)
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
        }

        public static void ApplyAllConfigurationsFromCurrentAssembly3(
            this ModelBuilder modelBuilder,
            Assembly assembly,
            string configNamespace = "")
        {
            var applyGenericMethods = typeof(ModelBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var applyGenericApplyConfigurationMethods = applyGenericMethods.Where(m => m.IsGenericMethod && m.Name.Equals("ApplyConfiguration", StringComparison.OrdinalIgnoreCase));
            var applyGenericMethod = applyGenericApplyConfigurationMethods.Where(m => m.GetParameters().FirstOrDefault().ParameterType.Name == "IEntityTypeConfiguration`1").FirstOrDefault();

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

            //IEnumerable<Type> assemblyTypeList;

            //switch (pAssemblyMethodType)
            //{
            //    case AssemblyMethodType.CallingAssembly:
            //        assemblyTypeList = Assembly.GetCallingAssembly()
            //            .GetTypes()
            //            .Where(c => c.IsClass
            //                && !c.IsAbstract
            //                && !c.ContainsGenericParameters);
            //        break;
            //    case AssemblyMethodType.ExecutingAssembly:
            //        assemblyTypeList = Assembly.GetExecutingAssembly()
            //            .GetTypes()
            //            .Where(c => c.IsClass
            //                && !c.IsAbstract
            //                && !c.ContainsGenericParameters);
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(pAssemblyMethodType), pAssemblyMethodType, null);
            //}
        }


        /// <summary>
        /// Applies all configurations defined in this assembly.
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="assembly">Optional. The assembly in which the config classes are located.</param>
        /// <param name="configNamespace">Optional. If provided, only configurations in this namespace will be applied.</param>
        public static void ApplyAllConfigurationsFromCurrentAssembly2(this ModelBuilder modelBuilder, Assembly assembly = null, string configNamespace = "")
        {
            var applyGenericMethods = typeof(ModelBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var applyGenericApplyConfigurationMethods = applyGenericMethods.Where(m => m.IsGenericMethod && m.Name.Equals("ApplyConfiguration", StringComparison.OrdinalIgnoreCase));
            var applyGenericMethod = applyGenericApplyConfigurationMethods.Where(m => m.GetParameters().FirstOrDefault().ParameterType.Name == "IEntityTypeConfiguration`1").FirstOrDefault();

            var callingAssembly = new StackFrame(1).GetMethod().DeclaringType.Assembly;
            var assemblyToUse = assembly ?? callingAssembly;
            var applicableTypes = assemblyToUse
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

        public static void SetGlobalMaxLength(this ModelBuilder modelBuilder, int maxLength = 200)
        {
            if (maxLength == default)
            {
                throw new ArgumentNullException(nameof(maxLength));
            }

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties().Where(p => p.ClrType == typeof(string)))
                {
                    property.SetMaxLength(maxLength);
                }
            }
        }

        public static void SetGlobalTablePrefix(this ModelBuilder modelBuilder, string prefix, bool useLowercase = true)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = useLowercase ? $"{prefix}entity.ClrType.Name".ToLower() : $"{prefix}entity.ClrType.Name";
                modelBuilder.Entity(entity.Name).ToTable(tableName);
            }
        }


        public static IEnumerable<IMutableEntityType> EntityTypes(this ModelBuilder builder)
        {
            return builder.Model.GetEntityTypes();
        }

        public static IEnumerable<IMutableProperty> Properties(this ModelBuilder builder)
        {
            return builder.EntityTypes().SelectMany(entityType => entityType.GetProperties());
        }

        public static IEnumerable<IMutableProperty> Properties<T>(this ModelBuilder builder)
        {
            return builder.EntityTypes().SelectMany(entityType => entityType.GetProperties().Where(x => x.ClrType == typeof(T)));
        }

        public static void Configure(this IEnumerable<IMutableEntityType> entityTypes, Action<IMutableEntityType> convention)
        {
            foreach (var entityType in entityTypes)
            {
                convention(entityType);
            }
        }

        public static void Configure(this IEnumerable<IMutableProperty> propertyTypes, Action<IMutableProperty> convention)
        {
            foreach (var propertyType in propertyTypes)
            {
                convention(propertyType);
            }
        }

        internal static bool HasAttribute<t>(this IMutableProperty property)
        {
            return property.PropertyInfo
                .GetCustomAttributes(false)
                .OfType<t>()
                .Any();
        }

        internal static t GetAttribute<t>(this IMutableProperty property)
        {
            return property.PropertyInfo
                .GetCustomAttributes(false)
                .OfType<t>()
                .First();
        }

        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Skip shadow types
                if (entityType.ClrType == null) continue;

                entityType.Relational().TableName = entityType.DisplayName();
                entityType.Relational().TableName = entityType.ClrType.Name;
            }
        }

        public static void SetSQLDefaultValues(this ModelBuilder builder)
        {
            builder
                .Properties()
                .Where(x => x.HasAttribute<SqlDefaultValueAttribute>())
                .Configure(c => c.Relational().DefaultValueSql = c.GetAttribute<SqlDefaultValueAttribute>().DefaultValue);
        }



        public static void DecimalPrecision(this ModelBuilder modelBuilder)
        {
            //foreach (var property in modelBuilder.Metadata.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties()))
            //{
            //    property.Builder.HasAnnotation(
            //        CoreAnnotationNames.TypeMapping,
            //        _typeMappingSource.FindMapping(property),
            //        ConfigurationSource.Convention);
            //}


            foreach (var decimalProperty in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetProperties())
                .Where(p => p.PropertyInfo != null
                            && (p.ClrType == typeof(decimal)
                                || p.ClrType == typeof(decimal?))))
            {
                var attribute = (DecimalPrecisionAttribute)decimalProperty.PropertyInfo
                    .GetCustomAttributes(typeof(DecimalPrecisionAttribute), true)
                    .FirstOrDefault();

                if (attribute != null)
                {
                    decimalProperty.Relational().ColumnType = $"decimal({attribute.Precision},{attribute.Scale})";
                }
            }
        }
        public abstract class EntityTypeConfiguration<TEntity> where TEntity : class
        {
            public abstract void Map(EntityTypeBuilder<TEntity> builder);
        }
        public static void AddConfiguration<TEntity>(this ModelBuilder modelBuilder, EntityTypeConfiguration<TEntity> configuration) where TEntity : class
        {
            configuration.Map(modelBuilder.Entity<TEntity>());
        }
    }
}
