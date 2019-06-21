using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using WebApi.Models;

namespace WebApi
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
            services.AddDbContext<DatabaseContext>(options =>
                {
                    options.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\fabio\\source\\repos\\WebApi\\WebApi\\Database\\Northwind.mdf;Integrated Security=True");
                });

            services.AddCors();
            services.AddOData();
            //services.AddDbContext<DatabaseContext>(opt => opt.UseInMemoryDatabase());
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(options => { options.AllowAnyOrigin(); });

            app.UseHttpsRedirection();

            app.UseMvc(
                build =>
                {
                    build.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                    build.MapODataServiceRoute(routeName: "odata", routePrefix: null, model: RegisterEdmModel());
                }
                );
        }

        public IEdmModel RegisterEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            // Entity Sets
            builder.EntitySet<Employee>("Employees");

            // Singleton
            builder.Singleton<Company>("Northwind");

            // Action
            builder.EntityType<Employee>()
                .Action("UpdateNotes")
                .Parameter<string>("Notes");

            // Bound Funtion
            builder.EntityType<Employee>().Collection
                .Function("SellToHomeTown")
                .ReturnsCollection<Employee>();

            // Unbound Function
            builder.Function("GetRandomRegion")
                .Returns<Region>();
            return builder.GetEdmModel();
        }
    }
}
