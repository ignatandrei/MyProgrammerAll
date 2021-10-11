using Buildalyzer;
using Buildalyzer.Workspaces;
using MyProgrammerAll;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MyProgrammerVSProjects
{
    public class FindProjects
    {
        /// <summary>
        /// https://github.com/boegholm/FixVSOpenRecent/blob/master/FixVSOpenRecent/Program.cs
        /// </summary>
        /// <returns></returns>
        public MyApp[] Projects2019()
        {
            var f= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            f = Path.Combine(f, "Microsoft", "VisualStudio");
            var files = Directory.GetFiles(f, "ApplicationPrivateSettings.xml", SearchOption.AllDirectories);
            if (files.Length == 0)
                return null;
            var fi = files.Select(it => new FileInfo(it)).ToArray();
            var last = fi[0];
            foreach (var item in fi)
            {
                if (last.LastWriteTimeUtc < item.LastWriteTimeUtc)
                    last = item;
            }
            var doc = XDocument.Load(last.FullName);
            var recentNode = doc.XPathSelectElement("/content/indexed/collection[@name='CodeContainers.Offline']/value");
            var val = recentNode.Value;
            var obj=JsonDocument.Parse(val).RootElement;
            var lst = new List<string>();
            foreach(var item in obj.EnumerateArray())
            {
                lst.Add(item.GetProperty("Key").GetString());
            }
            lst.Sort();
            var filesToAnalyze= lst.ToArray();
            var ret = new List<MyApp>();
            foreach (var item in files)
            {
                var app =Analyze(item);
                if(app != null)
                {
                    ret.Add(app);
                }
            }
            return ret.ToArray();
        }

        public MyApp Analyze(string file)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"{file} does not exists");
                return null;
            }
            if (!file.EndsWith(".sln"))
            {
                Console.WriteLine("analyze just sln files");
                return null;
            }
            var myApp = new MyApp();
            

            var manager = new AnalyzerManager(file);
            
            var ws = manager.GetWorkspace();
            var sol = ws.CurrentSolution;
            var dep = sol.GetProjectDependencyGraph();
            foreach(var projID in dep.GetTopologicallySortedProjects())
            {

                var proj = sol.GetProject(projID);
                var refPrj = proj.ProjectReferences.ToArray();
                var x = refPrj.Length;
                var m = proj.MetadataReferences.ToArray();
                var x1 = m.Length;
                //var projPack = manager.SolutionFile.ProjectsByGuid[projID.Id.ToString("D")];
                //var pack=projPack.

            }
            return myApp;
            

        }
    }
}
