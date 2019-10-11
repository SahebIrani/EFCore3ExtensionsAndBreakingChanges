using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WebApp.Models;

using static Sample.ModelBuilderExtensions;

namespace WebApp.Data
{
    public class PersonConfiguration : EntityMappingConfiguration<Person>
    {
        public override void Map(EntityTypeBuilder<Person> b)
        {
            b.ToTable("Person", "HumanResources")
                .HasKey(p => p.Id);

            b.Property(p => p.FullName).HasMaxLength(50).IsRequired();
        }
    }
}
