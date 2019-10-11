using System.Linq;
using System.Reflection;

using Inflector;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Sample;

using WebApp.Models;

using static Sample.ModelBuilderExtensions;

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

            modelBuilder.SetGlobalMaxLength(200);
            modelBuilder.SetGlobalTablePrefix("tbl_");

            Assembly assemblyWithConfig = Assembly.GetExecutingAssembly();
            Assembly assemblyWithConfig2 = typeof(ApplicationDbContext).Assembly;
            Assembly assemblyWithConfigurations = GetType().Assembly;
            modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
            modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfig2);
            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly(assemblyWithConfigurations);
            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly2(assemblyWithConfigurations);
            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly3(assemblyWithConfig2);
            modelBuilder.ApplyAllConfigurationsFromAssembly(assemblyWithConfigurations);
            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly(assemblyWithConfig);
            modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfig);


            // equivalent of modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.EntityTypes()
                        .Configure(et => et.Relational().TableName = et.DisplayName());

            // Put the table name on the primary key
            modelBuilder.Properties()
                        .Where(x => x.Name == "Id")
                        .Configure(p => p.Relational().ColumnName = p.DeclaringEntityType.Name + "Id");

            // Mark timestamp columns as concurrency tokens
            modelBuilder.Properties()
                        .Where(x => x.Name == "Timestamp")
                        .Configure(p => p.IsConcurrencyToken = true);

            // equivalent of modelBuilder.Conventions.AddFromAssembly(Assembly.GetExecutingAssembly());
            // look at this answer: https://stackoverflow.com/a/43075152/3419825

            // for the other conventions, we do a metadata model loop
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // equivalent of modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
                entityType.Relational().TableName = entityType.DisplayName();

                // equivalent of modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
                // and modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
                entityType.GetForeignKeys()
                    .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
                    .ToList()
                    .ForEach(fk => fk.DeleteBehavior = DeleteBehavior.Restrict);
            }


            Type[] types = typeof(EntityTypeConfiguration<>).GetTypeInfo().Assembly.GetTypes();
            IEnumerable<Type> typesToRegister = types
                .Where(type => !string.IsNullOrEmpty(type.Namespace) &&
                                type.GetTypeInfo().BaseType != null &&
                                type.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                                type.GetTypeInfo().BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                ModelBuilderExtensions.AddConfiguration(modelBuilder, configurationInstance);
            }



            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                modelBuilder.Entity(entity.Name).ToTable(entity.Name + "s");
            }


            var entityTypes = myDomainAssembly.DefinedTypes
                 .Where(ti => ti.IsClass && ti.IsPublic && !ti.IsAbstract && !ti.IsNested)

            foreach (var entityType in entityTypes)
            {
                modelBuilder.Entity(entityType).ToTable(entity.Name + "s");
            }




            //For EF Core 3.0, use this to set the TableName property(because entity.Relational() no longer exist):

            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.DisplayName());
            }

            modelBuilder.RemovePluralizingTableNameConvention();

            modelBuilder.Entity<Person>().Property(x => x.MyDecimal).HasColumnType("decimal(14, 2)");
            modelBuilder.DecimalPrecision();
            modelBuilder.SetSQLDefaultValues();

            modelBuilder.AddConfiguration(new BlogMap());


            var people = this.DbSet<Person>().ToList();

            var s1 = "Accessories".Singularize(); //produces "Accessory"
            var s2 = "XMLDetails".Singularize(); //produces "XMLDetail"
        }

        public class Blog
        {
            public int BlogId { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
        }

        public class BlogMap : EntityTypeConfiguration<Blog>
        {
            public override void Map(EntityTypeBuilder<Blog> builder)
            {
                builder.ToTable("tbl_blogs");

                builder.Property(b => b.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                builder.Property(b => b.Url)
                    .IsRequired()
                    .HasMaxLength(500);

                builder.HasIndex(b => b.Url);
            }
        }
    }
}
