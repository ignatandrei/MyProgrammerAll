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
        async static Task Main(string[] args)
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

            var httpClient = new HttpClient();
            var i = 0;
            while (i<2) // This app runs indefinitely. replace with actual application termination logic.
            {
                i++;
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // Replace with a name which makes sense for this operation.
                using (telemetryClient.StartOperation<RequestTelemetry>("operation"))
                {
                    logger.LogWarning("A sample warning message. By default, logs with severity Warning or higher is captured by Application Insights");
                    logger.LogInformation("Calling google.com");
                    var res = await httpClient.GetAsync("https://google.com");
                    logger.LogInformation("Calling google completed with status:" + res.StatusCode);
                    telemetryClient.TrackEvent("google call event completed");
                }
                Console.WriteLine(i);
                await Task.Delay(2000);
            }
            telemetryClient.Flush();
            await Task.Delay(2000);
        }

        async static Task<int> Main1(string[] args)
        {
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
               .SetResourceBuilder(ResourceBuilder.CreateDefault()
               .AddService("MyProgrammerConsole"))
               .AddSource("MyProgrammerBase.*")
               .AddConsoleExporter(c=>
               {
                   c.Targets = OpenTelemetry.Exporter.ConsoleExporterOutputTargets.Console;
               })
               .AddAzureMonitorTraceExporter(o =>
               {
                   o.ConnectionString = "InstrumentationKey=4772445f-40dd-44ae-b7d5-2c2ea33b9de3;IngestionEndpoint=https://westus2-2.in.applicationinsights.azure.com/";
                    
               })
               .Build();
            var rootCommand = new RootCommand("My programmer tools");
            var cmdExport = new Command("export", "export more features");
            var winget = new Command("programsWinget", "programs that are also winget");
            var vs2019sln = new Command("VS2019Sln", "VS2019 SLN");
            cmdExport.AddCommand(winget);
            cmdExport.AddCommand(vs2019sln);
            rootCommand.AddCommand(cmdExport);
            vs2019sln.Handler = CommandHandler.Create(async () => await FindSLN());
            winget.Handler = CommandHandler.Create(async () => await FindProgramsWinget());

            return await rootCommand.InvokeAsync(args);
            //var t = new Task[]
            //{
            //    FindProgramsWinget(),
            //    FindSLN()
            //};
            
            //await Task.WhenAll(t);
            //Console.WriteLine("finish");

        }
        async static Task FindSLN()
        {
            var p = new FindProjects();
            var prj = p.Projects2019();            
            await WriteToDisk(prj, "projects2019.txt");
        }
        async static Task FindProgramsWinget()
        {
            var l = new ListOfApps();
            Console.WriteLine("found "  + l.StartFind());
            var nr = Environment.ProcessorCount;
            //var throttler = new SemaphoreSlim(initialCount: nr);
            //TODO: use chunks in .NET 6
            //var nrData = (l.Count() / nr) * nr;
            //var arr = l.Take(nrData).ToArray();
            foreach (var item in l)
            {
                //await throttler.WaitAsync();
                await item.FindMoreDetails();
            }
            

            //var t = l.Select(it => it.FindMoreDetails()).ToArray();
            //await Task.WhenAll(t);            
            var parsed = l.ParsedWinGet();
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
