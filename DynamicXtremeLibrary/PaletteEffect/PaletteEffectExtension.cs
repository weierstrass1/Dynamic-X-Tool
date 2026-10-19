using DynamicXtremeLibrary.Logging.LoggingRegisters;
using DynamicXtremeLibrary.Defines;
using DynamicXtremeLibrary.Readers;
using DynamicXtremePaletteCreatorLibrary;
using LogRegister;
using System.Text;

namespace DynamicXtremeLibrary.PaletteEffec
{
    public class PaletteEffectExtension
    {
        public static IReadOnlyList<PaletteEffectCollection> GetCollections(string paletteDirectory,
            LogRegisterSystem log, out bool validation)
        {
            string[] paths = [.. Directory.GetFiles(paletteDirectory, "*.paleffect")
                .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ThenBy(path => Path.GetFileName(path), StringComparer.Ordinal)];
            PaletteEffectCollection? collection;
            List<PaletteEffectCollection> res = [];
            validation = AsarSymbolName.ValidateUnique(paths, log);
            foreach (var path in paths)
            {
                collection = new()
                {
                    Name = AsarSymbolName.FromPath(path)
                };
                if (collection.Load(path))
                {
                    bool collectionIsValid = true;
                    foreach (var effect in collection.Effects)
                    {
                        (string Parameter, int Value, int Limit)[] values =
                        [
                            ("Channel1", effect.Channel1, 31),
                            ("Channel2", effect.Channel2, 31),
                            ("Channel3", effect.Channel3, 31),
                            ("Ratio1", effect.Ratio1, 127),
                            ("Ratio2", effect.Ratio2, 127),
                            ("Ratio3", effect.Ratio3, 127)
                        ];
                        foreach (var value in values)
                        {
                            if (value.Value <= value.Limit)
                                continue;
                            log.Add(new ValueExceedsLimit(path, value.Parameter,
                                value.Value, value.Limit));
                            collectionIsValid = false;
                        }
                        if (!Enum.IsDefined(effect.EffectType))
                        {
                            log.Add(new ValueNotAllowed(path, "EffectType",
                                (int)effect.EffectType,
                                Enum.GetValues<EffectType>().Select(type => (int)type)));
                            collectionIsValid = false;
                        }
                    }
                    if (collectionIsValid)
                        res.Add(collection);
                    else
                        validation = false;
                }
                else
                {
                    validation = false;
                    log.Add(new FailedToProcessFile(path));
                }
            }
            return res.AsReadOnly();
        }
        public static void ToFile(LogRegisterSystem log, IEnumerable<PaletteEffectCollection> effects, string file)
        {
            if (effects == null || !effects.Any())
            {
                File.WriteAllText(file, "PaletteEffectsTable:\n.Length\n\tdw $0000\n");
                return;
            }
            List<int> type = [];
            List<int> c1 = [];
            List<int> c2 = [];
            List<int> c3 = [];
            List<int> r1 = [];
            List<int> r2 = [];
            List<int> r3 = [];
            int counter = 0;
            IEnumerable<PaletteEffect> filtered;
            log.Add(new Title("Palette Effects"));
            foreach (PaletteEffectCollection effect in effects)
            {
                filtered = effect.Effects.Where(x => x.EffectType != EffectType.None);
                log.Add(new InsertedPaletteEffect(effect.Name, filtered.Count()));
                type.AddRange(filtered.Select(x => (int)x.EffectType));
                c1.AddRange(filtered.Select(x => (int)x.Channel1));
                c2.AddRange(filtered.Select(x => (int)x.Channel2));
                c3.AddRange(filtered.Select(x => (int)x.Channel3));
                r1.AddRange(filtered.Select(x => (int)x.Ratio1));
                r2.AddRange(filtered.Select(x => (int)x.Ratio2));
                r3.AddRange(filtered.Select(x => (int)x.Ratio3));
                counter += filtered.Count();
            }
            StringBuilder sb = new();
            sb.AppendLine($"PaletteEffectsTable:\n.Length\n\tdw ${counter:X4}\n");
            sb.Append(".Types:");
            sb.AppendLine(HexReader.ValuesToString(type.ToArray(), 2));
            sb.AppendLine();
            sb.Append(".Value1");
            sb.AppendLine(HexReader.ValuesToString(c1.ToArray(), 2));
            sb.AppendLine();
            sb.Append(".Value2");
            sb.AppendLine(HexReader.ValuesToString(c2.ToArray(), 2));
            sb.AppendLine();
            sb.Append(".Value3");
            sb.AppendLine(HexReader.ValuesToString(c3.ToArray(), 2));
            sb.AppendLine();
            sb.Append(".Ratio1");
            sb.AppendLine(HexReader.ValuesToString(r1.ToArray(), 2));
            sb.AppendLine();
            sb.Append(".Ratio2");
            sb.AppendLine(HexReader.ValuesToString(r2.ToArray(), 2));
            sb.AppendLine();
            sb.Append(".Ratio3");
            sb.AppendLine(HexReader.ValuesToString(r3.ToArray(), 2));
            File.WriteAllText(file, sb.ToString());
        }
        public static byte[] ToBin(IEnumerable<PaletteEffectCollection> effects)
        {
            if(effects == null || effects.Count() == 0)
                return [0, 0, 0, 0];
            List<byte> type = [];
            List<byte> c1 = [];
            List<byte> c2 = [];
            List<byte> c3 = [];
            List<byte> r1 = [];
            List<byte> r2 = [];
            List<byte> r3 = [];
            int counter = 0;
            IEnumerable<PaletteEffect> filtered;
            foreach (PaletteEffectCollection effect in effects)
            {
                filtered = effect.Effects.Where(x => x.EffectType != EffectType.None);
                //Log.WriteLine($"{effect.Name}: {filtered.Count()}");
                type.AddRange(filtered.Select(x => (byte)x.EffectType));
                c1.AddRange(filtered.Select(x => x.Channel1));
                c2.AddRange(filtered.Select(x => x.Channel2));
                c3.AddRange(filtered.Select(x => x.Channel3));
                r1.AddRange(filtered.Select(x => x.Ratio1));
                r2.AddRange(filtered.Select(x => x.Ratio2));
                r3.AddRange(filtered.Select(x => x.Ratio3));
                counter += filtered.Count();
            }
            List<byte> res =
            [
                (byte)counter,
                (byte)(counter / 256),
                .. type,
                .. c1,
                .. c2,
                .. c3,
                .. r1,
                .. r2,
                .. r3,
            ];
            return [.. res];
        }
    }
}
