using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using StaticDotNet.EntityFrameworkCore.ModelConfiguration;

using WebApp.Data;
using WebApp.Services;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Assembly[] assemblies = new Assembly[]
            {
                // Add your assembiles here.
                typeof(Startup).GetTypeInfo().Assembly
            };

            //IEntityTypeConfigurationProvider provider = new MyEntityTypeConfigurationProvider();

            services.AddDbContextPool<ApplicationDbContext>(options => options
               .UseInMemoryDatabase("SinjulMSBH")
               //.Configure(fluently => fluently
               // //.Using(type => type.Namespace.EndsWith("Entities"))
               // .Using(new Sample.MyEntityAutoConfiguration())
               // .AddEntitiesFromAssemblyOf<ApplicationDbContext>())
               .RalmsExtendFunctions()
               .AddEntityTypeConfigurations(assemblies) //provider
            );

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages();


            //services.AddSingleton<IPluralizer, Pluralizer>();
            services.AddTransient<IService, Service>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
