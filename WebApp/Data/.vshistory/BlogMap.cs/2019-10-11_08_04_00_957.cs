using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WebApp.Models;

using static Sample.ModelBuilderExtensions;

namespace WebApp.Data
{
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
