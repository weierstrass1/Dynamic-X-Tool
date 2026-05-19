using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DynamicXtremeLibrary.Asar
{
    public class AsarPatch
    {
        public async static Task<(bool, string)> Run()
        {
            string asarPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(AppContext.BaseDirectory, "Asar", "asar.exe")
                : Path.Combine(AppContext.BaseDirectory, "Asar", "asar");

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
    }
}
