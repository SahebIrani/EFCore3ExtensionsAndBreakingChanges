using System;

using FluentModelBuilder.Configuration;

namespace Sample
{
    public class MyEntityAutoConfiguration : DefaultEntityAutoConfiguration
    {
        public override bool ShouldMap(Type type)
        {
            //return base.ShouldMap(type) && type.GetTypeInfo().IsSubclassOf(typeof(Entity));
        }
    }
}
