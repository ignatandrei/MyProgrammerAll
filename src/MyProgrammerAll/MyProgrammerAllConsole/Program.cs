using MyProgrammerAll;
using MyProgrammerBase;
using MyProgrammerVSProjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyProgrammerAllConsole
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var t = new Task[]
            {
                FindPrograms(),
                FindSLN()
            };
            
            await Task.WhenAll(t);

        }
        async static Task FindSLN()
        {
            var p = new FindProjects();
            var prj = p.Projects2019();            
            await WriteToDisk(prj, "projects2019.txt");
        }
        async static Task FindPrograms()
        {
            var l = new ListOfApps();
            Console.WriteLine("found "  + l.StartFind());
            var t = new List<Task>();
            foreach (var item in l)
            {
                //if(item.Name.Contains("Zip"))
                {
                    item.FindMoreDetails();
                    //break;
                }
            }
            var parsed = l.Where(it => !string.IsNullOrWhiteSpace(it.HomeURL)).ToArray();
            Console.WriteLine("parsed" + parsed.Length);
            foreach (var item in parsed)
            {
                Console.WriteLine(item.ToString());
            }

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
