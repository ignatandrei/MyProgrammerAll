using AOPMethodsCommon;
using Buildalyzer;
using Buildalyzer.Workspaces;
using MyProgrammerBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MyProgrammerVSProjects
{
    [AutoMethods(template = TemplateMethod.CustomTemplateFile, MethodPrefix = "auto", CustomTemplateFileName = "../AutoMethod.txt")]

    public partial class FindProjects
    {
        /// <summary>
        /// https://github.com/boegholm/FixVSOpenRecent/blob/master/FixVSOpenRecent/Program.cs
        /// </summary>
        /// <returns></returns>
        [AOPMarkerMethod]
        public IBaseUseApp[] Projects2019()
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
            var ret = new List<IBaseUseApp>();
            foreach (var item in filesToAnalyze)
            {
                try
                {
                    var app = Analyze(item);
                    if (app != null)
                    {
                        ret.AddRange(app);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("error " + ex.Message);
                }
                
            }
            return ret.ToArray();
        }
        [AOPMarkerMethod]
        bool VerifyIfProjectExists(string file)
        {
            try
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"{file} does not exists");
                    return false;
                }
                if (!file.EndsWith(".sln"))
                {
                    Console.WriteLine($"analyze just sln files, not {file} ");
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        [AOPMarkerMethod]
        public IBaseUseApp[] Analyze(string file)
        {
            Console.WriteLine($"start {file}");
            if (!VerifyIfProjectExists(file))
                return null;

            var ret = new List<IBaseUseApp>();

           
            var manager = new AnalyzerManager(file);
            var projsBuildLyzer = manager.Projects;
            var ws = manager.GetWorkspace();
             
            var sol = ws.CurrentSolution;
            
            var dep = sol.GetProjectDependencyGraph();


            var newSln = new ProjectReference();
            ret.Add(newSln);
            newSln.Type = "solution";
            newSln.ID = sol.Id.Id.ToString("D");
            newSln.Source = file;
            newSln.Name = Path.GetFileNameWithoutExtension(file);
            foreach (var projID in dep.GetTopologicallySortedProjects())
            {
                var newProj = new ProjectReference();
                ret.Add(newProj);
                newProj.ID = projID.Id.ToString("D");
                newProj.Type = "project";
                
                var proj = sol.GetProject(projID);
                newProj.Name = Path.GetFileNameWithoutExtension(proj.FilePath);
                newProj.Version = proj.Version.ToString();
                newProj.Source = proj.FilePath;                
                
                var refPrj = proj.ProjectReferences.ToArray();
                if (refPrj.Length > 0)
                {
                    newProj.ProjectReferences = refPrj.Select(it => it.ProjectId.Id.ToString("D")).ToArray();                    
                }
                var projBUild = projsBuildLyzer.First(it => it.Key == newProj.Source).Value.ProjectFile;
                if (projBUild.ContainsPackageReferences) {

                    newProj.PackageReferences= projBUild.PackageReferences.Select(item =>
                    {

                        var packRef = new NuGetReference();
                        packRef.Type = "package";
                        packRef.ID = item.Name;
                        packRef.Version = item.Version;
                        return packRef;

                    }).ToArray();
                    ret.AddRange(newProj.PackageReferences);
                }

            }
            return ret.ToArray();
            

        }
    }
}
