using System.Diagnostics;
using System.ComponentModel;

namespace DynamicXtremeLibrary.Asar
{
    /// <summary>Runs the bundled Asar executable against the generated Dynamic Xtreme patch.</summary>
    public class AsarPatch
    {
        /// <summary>Applies the patch to the temporary ROM and captures Asar output.</summary>
        /// <returns>The process success flag and combined standard output and error text.</returns>
        public async static Task<(bool, string)> Run()
        {
            try
            {
                return await RunProcess();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or Win32Exception)
            {
                return (false, $"Could not execute bundled Asar: {exception.Message}. " +
                    "Check that the binary exists, has execute permission, and its native dependencies are installed.");
            }
        }

        /// <summary>Starts Asar, drains both output streams concurrently, and checks its exit code.</summary>
        /// <returns>Whether Asar exited successfully and its complete diagnostic output.</returns>
        private static async Task<(bool, string)> RunProcess()
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

            await process.WaitForExitAsync();

            string stdout = await readOut;
            string stderr = await readErr;

            bool success = process.ExitCode == 0;
            if (!success && string.IsNullOrWhiteSpace(stdout + stderr))
                stderr = $"Asar exited with code {process.ExitCode} without diagnostic output.";

            return (success, stdout + stderr);
        }
        /// <summary>Verifies the bundled binary and restores a missing execute bit on Linux as a fallback.</summary>
        /// <returns>The executable path to launch.</returns>
        private static string EnsureExecutable()
        {
            string asarPath = Path.Combine(AppContext.BaseDirectory, "Asar",
                OperatingSystem.IsLinux() ? "asar" : "asar.exe");
            if (!File.Exists(asarPath))
                throw new FileNotFoundException("Bundled Asar binary was not found", asarPath);
            if (OperatingSystem.IsLinux())
            {
                UnixFileMode mode = File.GetUnixFileMode(asarPath);
                const UnixFileMode executable = UnixFileMode.UserExecute |
                    UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
                if ((mode & executable) == 0)
                {
                    File.SetUnixFileMode(asarPath, mode | UnixFileMode.UserExecute);
                    if ((File.GetUnixFileMode(asarPath) & executable) == 0)
                        throw new UnauthorizedAccessException($"Could not grant execute permission to '{asarPath}'");
                }
            }
            return asarPath;
        }
    }
}
