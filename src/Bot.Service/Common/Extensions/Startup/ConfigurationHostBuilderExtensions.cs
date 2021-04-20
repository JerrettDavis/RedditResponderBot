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
        /// <param name="hostBuilder">The application host builder</param>
        /// <param name="args">The args to pass to the commandline configuration</param>
        /// <returns>The configured host builder</returns>
        public static IHostBuilder AddConfiguration(
            this IHostBuilder hostBuilder,
            string[] args)
        {
            return hostBuilder.ConfigureAppConfiguration((hostContext, builder) =>
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddDotEnvFile(".env");
                builder.AddJsonFile("appsettings.json", false, true);
                builder.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true);
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