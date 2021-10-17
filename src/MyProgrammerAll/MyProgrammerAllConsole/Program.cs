using Azure.Monitor.OpenTelemetry.Exporter;
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyProgrammerAllConsole
{
    class Program
    {
        async static Task<int> Main(string[] args)
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
