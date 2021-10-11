using System;
using System.Diagnostics;

namespace MyProgrammerAll
{
    [StructGenerators.GenerateToString()]
    public partial class MyApp
    {
        public string ID { get; set; }
        public string WinGetID { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Publisher { get; set; }
        public string Type { get; set; }
        public MyApp ParentApp { get; set; }
        public bool Parsed { get; set; }
        public string HomeURL { get; set; }
        public void FindMoreDetails(){
            Console.WriteLine("start " + Name);
            Parsed = StartFindWinGet();
            
        }
        /// <summary>
        ///Name  Id        Version Source 
        ///7-Zip 7zip.7zip 19.00   winget
        /// </summary>
        /// <returns></returns>
        private string WingetFindId()
        {
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c winget list --accept-source-agreements --name \"{Name}\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            p.Start();
            p.WaitForExit();
            var output = p.StandardOutput.ReadToEnd();
            if (string.IsNullOrWhiteSpace(output))
                return null;
            var lines = output.Split(Environment.NewLine);
            if (lines.Length != 4)
            {
                if (lines.Length > 4)
                {
                    Console.WriteLine(output);
                }
                return null;
            }
            var header = lines[0].Trim();
            while (header.Length > 0 && !header.StartsWith("Name"))
                header = header.Substring(1);
            //Console.WriteLine(header);
            var id = header.IndexOf("Id");
            var vers = header.IndexOf("Version");
            //Console.WriteLine(id);
            //Console.WriteLine(vers);
            return lines[2].Substring(id,vers-id).Trim();
        }
        private void FindDetailsWinGet()
        {
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c winget show --accept-source-agreements --id \"{WinGetID }\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            p.Start();
            p.WaitForExit();
            var output = p.StandardOutput.ReadToEnd();
            if (string.IsNullOrWhiteSpace(output))
                return;
            var lines = output.Split(Environment.NewLine);
            foreach (var line in lines)
            {
                if (line.Contains("Homepage:"))
                {
                    HomeURL = line.Substring(line.IndexOf(":") + 1).Trim();
                    continue;
                }
                if (line.Contains("Description:"))
                {
                    Description = line.Substring(line.IndexOf(":") + 1).Trim();
                    continue;
                }
            }
            

        }
        public bool StartFindWinGet()
        {
            string full = WingetFindId();
            if (string.IsNullOrWhiteSpace(full)) 
                return false;
            Console.WriteLine(full);            
            this.WinGetID = full;
            FindDetailsWinGet();
            return true;
        }
    }
}
