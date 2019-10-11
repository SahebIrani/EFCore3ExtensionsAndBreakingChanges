using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Sample;

using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public interface IBaseEntity
    {
        //DateTimeOffset InsertDate { get; set; }
        //bool IsDeleted { get; set; }
    }
    public abstract class BaseEntity<TKey> : IBaseEntity
    {
        //public TKey Id { get; set; }

        //public DateTimeOffset InsertDate { get; set; }
        //public bool IsDeleted { get; set; }
    }
    public abstract class BaseEntity : BaseEntity<int>
    {
    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }

        [Index] // <- Here!
        [Index(IsClustered = true)]
        public string FullName { get; set; }


        [Column(TypeName = "decimal(14,2)")]
        [DecimalPrecision(14, 2)]
        [Display(Name = "My Decimal")]
        public Decimal? MyDecimal { get; set; }

        [SqlDefaultValue("Hello World")]
        public string ClassProperty { get; set; }

        [SqlDefaultValue("getdate()")]
        public DateTime DateProperty { get; set; }
    }

    public class Entity01 : IBaseEntity
    {
        public int Entity01Id { get; set; }
    }
    public class Entity02 : IBaseEntity
    {
        public int Entity02Id { get; set; }
    }
    public class Entity03 : IBaseEntity
    {
        public int Entity03Id { get; set; }
    }
    public class Entity04 : IBaseEntity
    {
        public int Entity04Id { get; set; }
    }

}
