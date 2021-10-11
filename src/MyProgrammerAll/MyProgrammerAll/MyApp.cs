using MyProgrammerBase;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MyProgrammerAll
{
    [StructGenerators.GenerateToString()]
    public partial class MyApp: IBaseUseApp
    {
        public string ID { get; set; }
        public string WinGetID { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Publisher { get; set; }
        public string Type { get; set; }
        public string ParentAppID { get; set; }
        public bool Parsed { get; set; }
        public string HomeURL { get; set; }
        public async Task FindMoreDetails(){
            Console.WriteLine("start " + Name);
            Parsed = await StartFindWinGet();
            Console.WriteLine($"parsed {Name} :{Parsed}");
        }
        /// <summary>
        ///Name  Id        Version Source 
        ///7-Zip 7zip.7zip 19.00   winget
        /// </summary>
        /// <returns></returns>
        private async Task<string> WingetFindId()
        {
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c winget list --accept-source-agreements --name \"{Name}\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            p.Start();
            var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromMilliseconds(60_000));
            await p.WaitForExitAsync(source.Token);
            p.WaitForExit(10_1000);
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
        private async Task FindDetailsWinGet()
        {
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c winget show --accept-source-agreements --id \"{WinGetID }\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            p.Start();
            var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromMilliseconds(60_000));
            await p.WaitForExitAsync(source.Token);
            p.WaitForExit(10_1000);

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
        public async Task< bool> StartFindWinGet()
        {
            try
            {
                string full = await WingetFindId();
                if (string.IsNullOrWhiteSpace(full))
                    return false;
                Console.WriteLine(full);
                this.WinGetID = full;
                await FindDetailsWinGet();
                this.Type = "winget";
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("error " + ex.Message);
                return false;
            }
        }
    }
}
