using Jaeger;
using Jaeger.Samplers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;

namespace WebApplication2
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<ITracer>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                string serviceName = sp.GetRequiredService<IHostingEnvironment>().ApplicationName;

                var samplerConfiguration = new Configuration.SamplerConfiguration(loggerFactory)
                    .WithType(ConstSampler.Type)
                    .WithParam(1);

                var senderConfiguration = new Configuration.SenderConfiguration(loggerFactory)
                    .WithAgentHost("localhost")
                    .WithAgentPort(6831);

                var reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
                    .WithLogSpans(true)
                    .WithSender(senderConfiguration);

                var tracer = (Tracer)new Configuration(serviceName, loggerFactory)
                    .WithSampler(samplerConfiguration)
                    .WithReporter(reporterConfiguration)
                    .GetTracer();


                //GlobalTracer.Register(tracer);

                return tracer;
            });
            services.AddOpenTracing();
        }

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

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }    
}
