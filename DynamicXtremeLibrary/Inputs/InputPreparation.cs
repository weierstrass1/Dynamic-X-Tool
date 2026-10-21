using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.Readers;
using LogRegister;

namespace DynamicXtremeLibrary.Inputs
{
    /// <summary>
    /// Identifies every directory, external-tool list, and executable location used to discover
    /// Dynamic Xtreme input files.
    /// </summary>
    /// <param name="DynamicInfoDirectory">The directory containing traditional DynamicInfo inputs.</param>
    /// <param name="DrawInfoDirectory">The directory containing traditional DrawInfo inputs.</param>
    /// <param name="PaletteEffectsDirectory">The directory containing traditional palette-effect inputs.</param>
    /// <param name="DynamicResourcesDirectory">The resource directory used by traditional DynamicInfo inputs.</param>
    /// <param name="PixiListPath">The optional PIXI list whose ASM entries may include Dynamic Xtreme inputs.</param>
    /// <param name="PixiExecutablePath">The optional PIXI executable used to locate its installation directory.</param>
    /// <param name="UberasmListPath">The optional UberASMTool list whose ASM entries may include Dynamic Xtreme inputs.</param>
    /// <param name="UberasmExecutablePath">The optional UberASMTool executable used to locate its installation directory.</param>
    internal sealed record InputSources(
        string DynamicInfoDirectory,
        string DrawInfoDirectory,
        string PaletteEffectsDirectory,
        string DynamicResourcesDirectory,
        string? PixiListPath,
        string? PixiExecutablePath,
        string? UberasmListPath,
        string? UberasmExecutablePath);

    /// <summary>
    /// Selects the input categories that must be parsed and resolved for the enabled Dynamic
    /// Xtreme features.
    /// </summary>
    /// <param name="DynamicInfos">Whether DynamicInfo inputs are required.</param>
    /// <param name="DrawInfos">Whether DrawInfo inputs are required.</param>
    /// <param name="PaletteEffects">Whether palette-effect inputs are required.</param>
    internal readonly record struct EnabledInputs(
        bool DynamicInfos,
        bool DrawInfos,
        bool PaletteEffects);

    /// <summary>
    /// Coordinates physical input discovery and semantic input resolution before ROM generation
    /// begins.
    /// </summary>
    internal sealed class InputPreparation
    {
        private readonly LogRegisterSystem _log;
        private readonly string _temporaryDirectory;
        private readonly Func<NameCollision, bool>? _collisionResolver;

        /// <summary>
        /// Initializes an input-preparation operation.
        /// </summary>
        /// <param name="log">The log that receives discovery, validation, and collision errors.</param>
        /// <param name="temporaryDirectory">The directory available for temporary parsing data.</param>
        /// <param name="collisionResolver">The optional callback used to approve generated collision-free names.</param>
        public InputPreparation(LogRegisterSystem log, string temporaryDirectory,
            Func<NameCollision, bool>? collisionResolver)
        {
            _log = log;
            _temporaryDirectory = temporaryDirectory;
            _collisionResolver = collisionResolver;
        }

        /// <summary>
        /// Discovers inputs from the traditional directories and external-tool lists, then parses,
        /// deduplicates, validates, and assigns their final logical names.
        /// </summary>
        /// <param name="sources">The locations from which inputs and their consumers are discovered.</param>
        /// <param name="enabledInputs">The input categories required by the current installation.</param>
        /// <returns>The completely resolved inputs, or <see langword="null"/> when preparation fails.</returns>
        public ResolvedInputs? Prepare(InputSources sources, EnabledInputs enabledInputs)
        {
            try
            {
                IReadOnlyList<InfoInput> discoveredInputs = ExternalInputDiscovery.Discover(
                    sources.DynamicInfoDirectory,
                    sources.DrawInfoDirectory,
                    sources.PaletteEffectsDirectory,
                    sources.DynamicResourcesDirectory,
                    sources.PixiListPath,
                    sources.PixiExecutablePath,
                    sources.UberasmListPath,
                    sources.UberasmExecutablePath);

                InputResolver resolver = new(_log, new ReadInfo(_log), _temporaryDirectory,
                    _collisionResolver);
                return resolver.Resolve(discoveredInputs, enabledInputs.DynamicInfos,
                    enabledInputs.DrawInfos, enabledInputs.PaletteEffects);
            }
            catch (Exception exception)
            {
                _log.Add(new InputDiscoveryError(exception.Message));
                return null;
            }
        }
    }
}
