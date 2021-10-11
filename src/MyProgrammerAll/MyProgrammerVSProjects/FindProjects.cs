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
            foreach (var item in filesToAnalyze)
            {
                var app =Analyze(item);
                if(app != null)
                {
                    ret.AddRange(app);
                    return ret.ToArray();
                }
                
            }
            return ret.ToArray();
        }

        public MyApp[] Analyze(string file)
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
            Console.WriteLine($"start {file}");
            var ret = new List<MyApp>();

           
            var manager = new AnalyzerManager(file);
            var projsBuildLyzer = manager.Projects;
            var ws = manager.GetWorkspace();
             
            var sol = ws.CurrentSolution;
            
            var dep = sol.GetProjectDependencyGraph();


            var newSln = new MyApp();
            ret.Add(newSln);
            newSln.Type = "vs2019";
            newSln.ID = sol.Id.Id.ToString("D");
            newSln.Source = file;
            newSln.Name = Path.GetFileNameWithoutExtension(file);
            foreach (var projID in dep.GetTopologicallySortedProjects())
            {
                var newProj = new MyApp();
                ret.Add(newProj);
                newProj.ID = projID.Id.ToString("D");
                newProj.Type = "project";
                
                var proj = sol.GetProject(projID);
                newProj.Name = Path.GetFileNameWithoutExtension(proj.FilePath);
                newProj.Version = proj.Version.ToString();
                newProj.Source = proj.FilePath;                
                newProj.ParentAppID = newSln.ID;
                var refPrj = proj.ProjectReferences.ToArray();
                foreach (var item in refPrj)
                {
                    var projRef = new MyApp();
                    ret.Add(projRef);
                    projRef.Type = "project";
                    projRef.ID = item.ProjectId.Id.ToString("D");
                    projRef.ParentAppID = newProj.ID;

                }

                var projBUild = projsBuildLyzer.FirstOrDefault(it => it.Key == newProj.Source).Value.ProjectFile;
                if (projBUild.ContainsPackageReferences) {
                    foreach (var item in projBUild.PackageReferences )
                    {
                                                
                        var packRef = new MyApp();
                        ret.Add(packRef);
                        packRef.Type = "package";
                        packRef.ID = item.Name;
                        packRef.Version = item.Version;
                        packRef.ParentAppID = newProj.ID;

                    }
                }
                //var projPack = manager.SolutionFile.ProjectsByGuid[projID.Id.ToString("D")];
                //var pack=projPack.

            }
            return ret.ToArray();
            

        }
    }
}
