using DynamicXtremeLibrary.Inputs;
using DynamicXtremeLibrary.Logging.LoggingRegisters;
using LogRegister;

namespace DynamicXtremeLibrary.ExternalTools
{
    /// <summary>
    /// Applies every external source rewrite, writes the generated ROM, and updates external tools
    /// as one rollback-capable operation.
    /// </summary>
    internal sealed class InstallationCommitter
    {
        private readonly LogRegisterSystem _log;
        private readonly ExternalToolsLinker _linker;

        /// <summary>
        /// Initializes a transactional installation committer.
        /// </summary>
        /// <param name="log">The log that receives commit and rollback errors.</param>
        /// <param name="linker">The linker used to update PIXI, GPS, and UberASMTool files.</param>
        public InstallationCommitter(LogRegisterSystem log, ExternalToolsLinker linker)
        {
            _log = log;
            _linker = linker;
        }

        /// <summary>
        /// Materializes the planned ASM rewrites and applies them together with the generated ROM
        /// and external-tool updates. All applied file changes are rolled back if any step fails.
        /// </summary>
        /// <param name="sourceRewrites">The source rewrites prepared during input resolution.</param>
        /// <param name="temporaryRomPath">The path of the successfully patched temporary ROM.</param>
        /// <param name="outputRomPath">The final ROM path. Its previous contents are preserved as a backup.</param>
        /// <returns><see langword="true"/> when every change is committed; otherwise, <see langword="false"/>.</returns>
        public bool TryCommit(IEnumerable<SourceRewrite> sourceRewrites,
            string temporaryRomPath, string outputRomPath)
        {
            try
            {
                Dictionary<SourceRewrite, byte[]> rewrittenContent = sourceRewrites
                    .ToDictionary(rewrite => rewrite, rewrite => rewrite.BuildContent());
                byte[] rom = File.ReadAllBytes(temporaryRomPath);

                using FileTransaction transaction = new();
                foreach (var rewrite in rewrittenContent)
                    transaction.WriteAllBytes(rewrite.Key.Path, rewrite.Value);

                transaction.WriteAllBytes(outputRomPath, rom, $"{outputRomPath}.bak");
                _linker.PixiLinker(transaction);
                _linker.GPSLinker(transaction);
                _linker.UberASMToolLinker(transaction);
                transaction.Commit();
                return true;
            }
            catch (Exception exception)
            {
                _log.Add(new InputDiscoveryError(
                    $"transaction was rolled back: {exception.Message}"));
                return false;
            }
        }
    }
}
