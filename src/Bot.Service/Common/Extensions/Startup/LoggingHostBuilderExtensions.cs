using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Bot.Service.Common.Extensions.Startup
{
    public static class LoggingHostBuilderExtensions
    {
        /// <summary>
        /// Adds logging to the application
        /// </summary>
        /// <param name="builder">The application host builder</param>
        /// <returns>The host builder with logging added</returns>
        public static IHostBuilder AddLogging(this IHostBuilder builder)
        {
            return builder.ConfigureLogging((context, loggingBuilder) =>
            {
                loggingBuilder.ClearProviders();

                var configuration = new ConfigurationBuilder()
                    .AddDotEnvFile(".env")
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                    .Build();
                var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                loggingBuilder.AddSerilog(logger, dispose: true);
            });
        }

        
    }
}