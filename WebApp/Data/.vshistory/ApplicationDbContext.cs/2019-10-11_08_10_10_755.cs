using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Inflector;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

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

        //protected new DbSet<TEntity> Set<TEntity>() where TEntity : EntityBase
        //{
        //    return base.Set<TEntity>();
        //}

        public DbSet<Entity01> Entity01 { get; set; }
        public DbSet<Entity02> Entity02 { get; set; }
        public DbSet<Entity03> Entity03 { get; set; }
        public DbSet<Entity04> Entity04 { get; set; }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var service = this.GetService<IService>();
            var q = service.Print();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var service = this.GetService<IService>();
            var q = service.Print();

            return base.SaveChangesAsync(cancellationToken);
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
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                if (entity.Name.Contains("AspNet"))
                {
                    string tableSchema = entity.GetSchema();
                    entity.SetSchema(tableSchema + "_SinjulMSBH");

                    string tableName = entity.GetTableName().Replace("AspNet", "SinjulMSBH_");
                    entity.SetTableName(tableName);
                }
            }

            //if (entity.Name == "Simple.Models.Entities.Entity01")
            //{
            //    entity.SetDbSetName("Entity01");
            //    var a6 = entity.GetTableName();
            //    entity.SetTableName(a6 + "s");
            //    var a4 = entity.FindProperty("Entity1");
            //    entity.SetPrimaryKey(a4);
            //    //var a1 = entity.AddProperty("Test");
            //    var a2 = entity.FindPrimaryKey();
            //    var a3 = entity.FindProperties(new string[] { "Test,IsDeleted,InsertDate,Entity02Id" });
            //    var a44 = entity.FindProperty("InsertDate");
            //    var a7 = entity.GetType();
            //    var a8 = entity.PropertyCount();
            //    var a9 = entity.RelationshipPropertyCount();
            //    entity.SetAnnotation("DisplayName", "TestAlaki");
            //    var a5 = entity.GetAnnotation("DisplayName");
            //    //entity.SetQueryFilter(c => c.Name == "EntityTwo");
            //    //entity.Relational().TableName = entity.Relational().TableName.Replace("AspNet", "");
            //}
            //}


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
            //modelBuilder.EntityTypes().Configure(et => et.Relational().TableName = et.DisplayName());
            modelBuilder.EntityTypes().Configure(et => et.SetTableName(et.DisplayName()));

            // Put the table name on the primary key
            //modelBuilder.Properties().Where(x => x.Name == "Id").Configure(p => p.Relational().ColumnName = p.DeclaringEntityType.Name + "Id");
            modelBuilder.Properties().Where(x => x.Name == "Id").Configure(p => p.SetColumnName(p.DeclaringEntityType.Name + "Id"));

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
            modelBuilder.AddEntityConfigurationsFromAssembly(GetType().Assembly);


            var people = this.DbSet<Person>().ToList();

            var s1 = "Accessories".Singularize(); //produces "Accessory"
            var s2 = "XMLDetails".Singularize(); //produces "XMLDetail"
        }
    }
}
