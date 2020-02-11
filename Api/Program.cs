namespace Authentication
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = CreateHostBuilder(args).Build();
            await builder.Services.MigrateDatabaseAsync().ConfigureAwait(false);
            await builder.RunAsync().ConfigureAwait(false);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureLogging((context, builder) =>
            {
                var section = context.Configuration.GetSerilogOptionsSection();
                builder.ClearProviders().AddSerilog(context.Configuration, section);
                if (context.HostingEnvironment.IsProduction())
                {
                    builder.AddAzureLogging();
                }
            })
            .ConfigureWebHostDefaults(webBuilder => webBuilder
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    if (context.HostingEnvironment.IsProduction())
                    {
                        configBuilder.AddAzureKeyVault(keyVaultName: "crgolden");
                    }
                })
                .UseStartup<Startup>());
    }
}
