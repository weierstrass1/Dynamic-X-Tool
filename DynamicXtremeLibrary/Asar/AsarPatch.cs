using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DynamicXtremeLibrary.Asar
{
    public class AsarPatch
    {
        public async static Task<(bool, string)> Run()
        {
            string asarPath = EnsureExecutable();

            ProcessStartInfo psi = new()
            {
                FileName = asarPath,

                RedirectStandardOutput = true,
                RedirectStandardError = true,

                UseShellExecute = false,
                CreateNoWindow = true
            };

            psi.ArgumentList.Add("./Patch/DynamicXtreme.asm");
            psi.ArgumentList.Add("./TMP/tmp.smc");

            using Process process = new() { StartInfo = psi };

            process.Start();

            Task<string> readOut = process.StandardOutput.ReadToEndAsync();
            Task<string> readErr = process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            string stdout = await readOut;
            string stderr = await readErr;

            bool success = string.IsNullOrWhiteSpace(stderr);

            return (success, stdout + stderr);
        }
        private static string EnsureExecutable()
        {
            string asarPath = Path.Combine(AppContext.BaseDirectory, "Asar", "asar.exe");
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return asarPath;

            asarPath = Path.Combine(AppContext.BaseDirectory, "Asar", "asar");

            using Process chmod = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            chmod.StartInfo.ArgumentList.Add("+x");
            chmod.StartInfo.ArgumentList.Add(asarPath);

            chmod.Start();
            chmod.WaitForExit();

            return asarPath;
        }
    }
}
