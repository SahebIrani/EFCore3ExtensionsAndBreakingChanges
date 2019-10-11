using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; }


        [Column(TypeName = "decimal(14,2)")]
        public Decimal? MyDecimal { get; set; }
    }
}
