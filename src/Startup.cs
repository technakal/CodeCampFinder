using AutoMapper;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Linq;
using System.Reflection;

namespace CoreCodeCamp
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<CampContext>();
      services.AddScoped<ICampRepository, CampRepository>();
      services.AddAutoMapper(typeof(Startup));
      services.AddApiVersioning(opt =>
      {
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.DefaultApiVersion = new ApiVersion(1, 1);
        opt.ReportApiVersions = true;
        opt.ApiVersionReader = ApiVersionReader.Combine(
          new HeaderApiVersionReader("X-Version"), 
          new QueryStringApiVersionReader("v", "version", "ver"));
      });
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v2", new OpenApiInfo { Title = "CodeCampFinder", Version = "v2" });
        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
      });
      services.AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "CodeCampFinder v2");
      });

      app.UseMvc();
    }
  }
}
