using System.Diagnostics;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using ResiliencePatternsDotNet.Domain.Commands;
using ResiliencePatternsDotNet.Domain.Common;
using ResiliencePatternsDotNet.Domain.Services;
using ResiliencePatternsDotNet.Infra;
using Toxiproxy.Net;

namespace ResiliencePatternsDotNet.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // var _proxyProcess = new Process()
            // {
            //     StartInfo = new ProcessStartInfo(@".\toxiproxy-server-windows-amd64.exe"),
            // };
            // _proxyProcess.Start();
            
            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            services.AddMetrics();
            
            services.AddDbContext<ResiliencePatternsDotNetDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<IExecuteService, ExecuteService>();
            services.AddScoped<MetricService>();
            services.AddScoped<MetricsRegistry>();
            
            services.AddControllers()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            
            services.AddMediatR(typeof(SampleCommand).GetTypeInfo().Assembly);
            
            services.AddCors(e => e.AddPolicy("default",
                c => c.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()));
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ginder API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseCors("default");
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
