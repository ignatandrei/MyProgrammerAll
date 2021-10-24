using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyProgrammerAll;
using MyProgrammerBase;
using MyProgrammerVSProjects;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyProgrammerAllConsole
{
    class Program
    {
        async static Task MainTestTelemetry(string[] args)
        {
            //https://docs.microsoft.com/en-us/azure/azure-monitor/app/worker-service
            // Create the DI container.
            IServiceCollection services = new ServiceCollection();

            // Being a regular console app, there is no appsettings.json or configuration providers enabled by default.
            // Hence instrumentation key and any changes to default logging level must be specified here.
            services.AddLogging(loggingBuilder => loggingBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("Category", LogLevel.Information));
            services.AddApplicationInsightsTelemetryWorkerService(
                aiso =>
                {
                    aiso.ConnectionString = "InstrumentationKey=4772445f-40dd-44ae-b7d5-2c2ea33b9de3;IngestionEndpoint=https://westus2-2.in.applicationinsights.azure.com/";
                    aiso.EnableDebugLogger = true;
                    aiso.AddAutoCollectedMetricExtractor = true;
                }
                );

            // Build ServiceProvider.
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Obtain logger instance from DI.
            ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // Obtain TelemetryClient instance from DI, for additional manual tracking or to flush.
            var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();

            
            using (var a = new Activity("AndreiDependencyTelemetry"))
            {
                a.AddTag("E", "B");
                using (var h = telemetryClient.StartOperation<DependencyTelemetry>(a))
                {
                    await Task.Delay(2_000);
                    using (var a1 = new Activity("AndreiRequestTelemetry"))
                    {
                        a1.AddTag("Q", "T");
                        using (var h1 = telemetryClient.StartOperation<DependencyTelemetry>(a1))
                        {
                            await Task.Delay(2_000);
                            var res = await new HttpClient().GetAsync("https://google.com"); // this dependency will be captured by Application Insights.
                            logger.LogWarning("Response from google is:" + res.StatusCode); // this will be captured by Application Insights.
                            telemetryClient.TrackEvent("sampleevent");
                            await Task.Delay(2_000);
                            telemetryClient.TrackException(new ArgumentException("bing"));
                        }
                    }
                    await Task.Delay(2_000);
                    telemetryClient.TrackException(new ArgumentException("asd"));
                    await Task.Delay(2_000);
                }
                
            }
            telemetryClient.Flush();
            await Task.Delay(2000);
        }

        async static Task<int> Main(string[] args)
        {

            IServiceCollection services = new ServiceCollection();

            //services.AddLogging(loggingBuilder => loggingBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("Category", LogLevel.Information));
            services.AddApplicationInsightsTelemetryWorkerService(
                aiso =>
                {
                    aiso.ConnectionString = "InstrumentationKey=4772445f-40dd-44ae-b7d5-2c2ea33b9de3;IngestionEndpoint=https://westus2-2.in.applicationinsights.azure.com/";
                    aiso.EnableDebugLogger = true;
                    aiso.AddAutoCollectedMetricExtractor = true;
                }
                );

            services.AddSingleton<FindProjects>();
            services.AddSingleton<ListOfApps>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var rootCommand = new RootCommand("My programmer tools");
            var cmdExport = new Command("export", "export more features");
            var winget = new Command("programsWinget", "programs that are also winget");
            var vs2019sln = new Command("VS2019Sln", "VS2019 SLN");
            cmdExport.AddCommand(winget);
            cmdExport.AddCommand(vs2019sln);
            rootCommand.AddCommand(cmdExport);
            vs2019sln.Handler = CommandHandler.Create(async () => await FindSLN(serviceProvider.GetRequiredService<FindProjects>()));
            winget.Handler = CommandHandler.Create(async () => await FindProgramsWinget(serviceProvider.GetRequiredService<ListOfApps>()));

            return await rootCommand.InvokeAsync(args);
            //var t = new Task[]
            //{
            //    FindProgramsWinget(),
            //    FindSLN()
            //};
            
            //await Task.WhenAll(t);
            //Console.WriteLine("finish");

        }
        async static Task FindSLN(FindProjects p)
        {
            
            var prj = p.Projects2019();            
            await WriteToDisk(prj, "projects2019.txt");
        }
        async static Task FindProgramsWinget(ListOfApps l)
        {
            
            var parsed = await l.FindProgramsWinget();
            Console.WriteLine("parsed:" + parsed.Length);
            //foreach (var item in parsed)
            //{
            //    Console.WriteLine(item.ToString());
            //}

            await WriteToDisk(parsed, "programs.txt");
            
        }
        private static async Task WriteToDisk(IBaseUseApp[] apps, string file)
        {
            var json = JsonSerializer.Serialize(apps, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            file = $"{Environment.MachineName}_{file}";
            if (File.Exists(file))
                File.Delete(file);
            await File.WriteAllTextAsync(file, json);
            Process.Start("notepad.exe", file);
            
        }
    }
}
