using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DynamicXtremeLibrary.Asar
{
    public class AsarPatch
    {
        public static string Run()
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

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();

            process.WaitForExit();

            return stdout + stderr;
        }
    }
}
