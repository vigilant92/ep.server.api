using ep.server.api.Config;
using ep.server.api.Repositiory;
using ep.server.api.Repositiory.IRepository;
using ep.server.api.Services;
using ep.server.api.Servies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace ep.server.api
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

            services.AddControllers();

            services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<IPhotoRepository, PhotoRepository>();
            var config = new ExternalApiConfig();
            Configuration.Bind("ExternalApi", config);

            services.AddHttpClient("apiClient", c => c.BaseAddress = new System.Uri(config.Url));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ep.server.api",
                    Version = "v1",
                    Description = "A sample project to provide data from another external API",
                    Contact = new OpenApiContact() { Name = "Tanvir Samad", Email = "tanvirsamad.uk@gmail.com" },

                });
            });
            services.AddSingleton(new TelemetryActivitySource("ep.server.api"));
            services.AddScoped<ITelemetryService, TelemetryService>();
            services.AddScoped<IServerTimings, ServerTimings>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(config =>
                {
                    config.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
                    {
                        ["activated"] = false
                    };
                    config.SwaggerEndpoint("/swagger/v1/swagger.json", "ep.server.api v1");
                });
            }
            app.UseMiddleware<ServerTimingMiddleware>("https://*");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
