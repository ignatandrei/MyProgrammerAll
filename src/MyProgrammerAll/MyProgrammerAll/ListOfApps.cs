using Microsoft.ApplicationInsights;
using Microsoft.Win32;
using MyProgrammerBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyProgrammerAll
{
    public class ListOfApps : IEnumerable<MyApp>
    {
        public ListOfApps(TelemetryClient tc)
        {
            this.tc = tc;
        }
        private List<MyApp> apps = new List<MyApp>();
        private readonly TelemetryClient tc;

        public IEnumerator<MyApp> GetEnumerator()
        {
            return apps.GetEnumerator();
        }
        public IBaseUseApp[] ParsedWinGet()
        {
            return this.Where(it => !string.IsNullOrWhiteSpace(it.WinGetID))
                .GroupBy(it => it.WinGetID)
                //TODO: use MaxBy from .NET 6
                .Select(it => it.FirstOrDefault(a => a.HomeURL != null))
                .Where(it => it != null)
                .ToArray();
        }
        public int StartFind(){
            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using(RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
            {
                foreach(string subkey_name in key.GetSubKeyNames())
                {
                    using(RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        var name=subkey.GetValue("DisplayName")?.ToString();
                        if(string.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        var app=new MyApp(tc);
                        app.Name=name;
                        app.Version=subkey.GetValue("DisplayVersion")?.ToString();
                        app.Publisher=subkey.GetValue("Publisher")?.ToString();
                        app.ID = subkey_name;
                        apps.Add(app);
                        //Console.WriteLine(subkey.GetValue("DisplayName"));
                    }
                }
            }
            apps.Sort((x, y) => (x.Name??"").CompareTo((y.Name??"")));
            return apps.Count;
        }
        // public void StartFind()
        // {
        //     var winget=WinGetLocation();
        //     Console.WriteLine(winget);
        //     if(string.IsNullOrEmpty(winget))
        //     {
        //         Console.WriteLine("No apps found");
        //         return;
        //     }
        //     var p=new Process();
        //     p.StartInfo.FileName="cmd.exe";
        //     p.StartInfo.Arguments=$"/c start /B winget list --accept-source-agreements";
        //     p.StartInfo.UseShellExecute=false;
        //     p.StartInfo.RedirectStandardOutput=true;
        //     p.StartInfo.CreateNoWindow = false;
        //     p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
        //     p.Start();
        //     p.WaitForExit();
        //     var output = p.StandardOutput.ReadToEnd();
        //     Console.WriteLine("test");
        //     Console.WriteLine(output);
        //     Console.WriteLine("test");
        // }
        // private string WinGetLocation(){
        //     var p=new Process();
        //     p.StartInfo.FileName="where";
        //     p.StartInfo.Arguments="winget";
        //     p.StartInfo.UseShellExecute=false;
        //     p.StartInfo.RedirectStandardOutput=true;
        //     p.Start(); 
        //     p.WaitForExit();
        //     var output=p.StandardOutput.ReadToEnd();
        //     return output;
        // }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.apps.GetEnumerator();
        }
    }
}
