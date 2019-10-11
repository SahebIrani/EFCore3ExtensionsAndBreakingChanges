using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WebApp.Models;


namespace WebApp.Data
{
    public class PersonConfiguration : Sample.ModelBuilderExtensions.EntityMappingConfiguration<Person>
    {
        public override void Map(EntityTypeBuilder<Person> b)
        {
            b.ToTable("Person", "HumanResources")
                .HasKey(p => p.Id);

            b.Property(p => p.FullName).HasMaxLength(50).IsRequired();
        }
    }

    public class ExamplePersonConfiguration : StaticDotNet.EntityFrameworkCore.ModelConfiguration.EntityTypeConfiguration<Person>
    {
        public override void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.FullName)
                .HasMaxLength(10);
        }
    }
}
