using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Sample;

namespace WebApp.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; }


        [Column(TypeName = "decimal(14,2)")]
        [DecimalPrecision(14, 2)]
        [Display(Name = "My Decimal")]
        public Decimal? MyDecimal { get; set; }
    }
}
