using System;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using static Sample.ModelBuilderExtensions;

namespace WebApp.Models
{
    public abstract class BaseEntity
    {
        public DateTimeOffset InsertDate { get; set; }
        public bool IsDeleted { get; set; }
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

    public class Entity01 : BaseEntity
    {
        public int Entity01Id { get; set; }
    }
    public class Entity02 : BaseEntity
    {
        public int Entity02Id { get; set; }
    }
    public class Entity03 : BaseEntity
    {
        public int Entity03Id { get; set; }
    }
    public class Entity04 : BaseEntity
    {
        public int Entity04Id { get; set; }
    }

    public interface IService
    {
        string Print();
    }

    public class MyService : IService
    {
        public string Print()
        {
            return "OK";
        }
    }
}
