using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Sample
{
    public class MyDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection services) => services.AddSingleton<IPluralizer, Pluralizer>();
    }

    public class Pluralizer : IPluralizer
    {
        public string Pluralize(string name) => Inflector.Inflector.Pluralize(name) ?? name;

        public string Singularize(string name) => Inflector.Inflector.Singularize(name) ?? name;
    }
}
