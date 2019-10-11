using System;

namespace Sample
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DecimalPrecisionAttribute : Attribute
    {
        public Byte Precision { get; set; }
        public Byte Scale { get; set; }

        public DecimalPrecisionAttribute(Byte precision, Byte scale)
        {
            Precision = precision;
            Scale = scale;
        }
    }

    //public class DecimalPrecisionAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<DecimalPrecisionAttribute>
    //{
    //    public override void Apply(ConventionPrimitivePropertyConfiguration configuration, DecimalPrecisionAttribute attribute)
    //    {
    //        if (attribute.Precision < 1 || attribute.Precision > 38)
    //            throw new InvalidOperationException("Precision must be between 1 and 38.");

    //        if (attribute.Scale > attribute.Precision)
    //            throw new InvalidOperationException("Scale must be between 0 and the Precision value.");

    //        configuration.HasPrecision(attribute.Precision, attribute.Scale);
    //    }
    //}
}
