using System;
using System.Threading.Tasks;
using CommandMiddleware.Sample.Web.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandMiddleware.Sample.Web
{

    public class Startup
    {
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var handlers = new Handlers(_loggerFactory.CreateLogger<Handlers>());
            var commandProcessor = CommandProcessor
                .Use((x, y) => LogCommand(x, y, _loggerFactory.CreateLogger(nameof(LogCommand))))
                .Handle<AddItemToBasket>(handlers.Handle)
                .Handle<Checkout>(handlers.Handle)
                .Build()
                .AsHttpCommandDelegate();
            
            
            services.AddSingleton(commandProcessor);
        }
        
        private static async Task LogCommand(object command, Func<Task> next, ILogger logger)
        {
            logger.LogInformation($"-- Processing {command.GetType().Name} --");
            await next();
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