using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Sample
{
    public class MyDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection services)
        {
            services.AddSingleton<IPluralizer, MyPluralizer>();
        }
    }

    public class MyPluralizer : IPluralizer
    {
        public string Pluralize(string name)
        {
            return Inflector.Pluralize(name) ?? name; Inflector.Inflector.SetDefaultCultureFunc.
        }

        public string Singularize(string name)
        {
            return Inflector.Singularize(name) ?? name;
        }
    }
}
