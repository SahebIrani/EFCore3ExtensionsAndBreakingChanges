using System;

namespace WebApp.Models
{
    public abstract class BaseEntity
    {
        public DateTimeOffset InsertDate { get; set; }
        public bool IsDeleted { get; set; }
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
