using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Bot.Service.Common.Extensions.Startup
{
    public static class ConfigurationHostBuilderExtensions
    {
        /// <summary>
        /// Adds configuration to the <see cref="IHostBuilder"/>
        /// </summary>
        /// <param name="builder">The application host builder</param>
        /// <param name="args">The args to pass to the commandline configuration</param>
        /// <returns>The configured host builder</returns>
        public static IHostBuilder AddConfiguration(
            this IHostBuilder builder,
            string[] args)
        {
            return builder.ConfigureAppConfiguration((hostContext, builder) =>
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile("appsettings.json", false, true);
                builder.AddCommandLine(args);
                builder.AddEnvironmentVariables();

                if (hostContext.HostingEnvironment.IsDevelopment())
                {
                    builder.AddUserSecrets<Program>();
                }
            });
        }
    }
}