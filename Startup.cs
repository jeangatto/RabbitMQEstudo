using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitWebApp.AppSettings;
using RabbitWebApp.Consumers;
using RabbitWebApp.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RabbitWebApp
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
            services.AddHostedService<EmailMessageConsumer>();

            services.AddScoped<IEmailNotificationService, EmailNotificationService>();

            services.Configure<RabbitMQConfig>(Configuration.GetSection(nameof(RabbitMQConfig)),
                binderOptions => binderOptions.BindNonPublicProperties = true);

            services.Configure<KestrelServerOptions>(
                kestrelServerOptions => kestrelServerOptions.AddServerHeader = false);

            services.Configure<RouteOptions>(routeOptions =>
            {
                routeOptions.LowercaseUrls = true;
                routeOptions.LowercaseQueryStrings = true;
            });

            services.AddControllers()
                .AddJsonOptions(jsonOptions =>
                {
                    jsonOptions.JsonSerializerOptions.WriteIndented = false;
                    jsonOptions.JsonSerializerOptions.AllowTrailingCommas = false;
                    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    jsonOptions.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                    jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                });

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RabbitMQ",
                Version = "v1"
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RabbitMQ v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}