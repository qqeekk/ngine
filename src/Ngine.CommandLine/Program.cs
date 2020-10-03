using Autofac;
using Autofac.Extensions.DependencyInjection;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngine.CommandLine.Infrastructure;
using Ngine.CommandLine.Options;
using Ngine.Infrastructure.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ngine.CommandLine
{
    internal class Program
    {
        private async static Task Main(string[] args)
        {
            var configuration = DefaultConfigurationBuilder.Create("appsettings.json").Build();

            var services = new ServiceCollection();
            services.AddLogging(options =>
            {
                options.AddConfiguration(configuration.GetSection("Logging"));
                options.AddConsole();
            });

            services.AddOptions();
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            var autofacContainerBuilder = new ContainerBuilder();
            autofacContainerBuilder.RegisterModule<NgineDependenciesModule>();
            autofacContainerBuilder.Populate(services);

            var autofacServiceProvider = new AutofacServiceProviderFactory()
                .CreateServiceProvider(autofacContainerBuilder);

            try
            {
                using var app = new CommandLineApplication<NgineApplication>();
                app.Conventions
                    .UseDefaultConventions()
                    .UseConstructorInjection(autofacServiceProvider);

                var tokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (s, e) => tokenSource.Cancel();
                await app.ExecuteAsync(args, tokenSource.Token);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);

                if (ex is UnrecognizedCommandParsingException uex && uex.NearestMatches.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("Возможно вы имели в виду:");
                    Console.WriteLine("    " + uex.NearestMatches.First());
                }
            }
        }
    }
}
