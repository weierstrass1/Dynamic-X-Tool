using DynamicXtremeLibrary.Readers;
using DynamicXtremePaletteCreatorLibrary;
using System.Text;

namespace DynamicXtremeLibrary.PaletteEffec
{
    public class PaletteEffectExtension
    {
        public static IReadOnlyList<PaletteEffectCollection> GetCollections(string paletteDirectory)
        {
            string[] paths = Directory.GetFiles(paletteDirectory, "*.paleffect");
            PaletteEffectCollection? collection;
            List<PaletteEffectCollection> res = [];
            foreach (var path in paths)
            {
                collection = new()
                {
                    Name = Path.GetFileNameWithoutExtension(path)
                };
                if (collection.Load(path))
                    res.Add(collection);
            }
            return res.AsReadOnly();
        }
        public static void ToFile(IEnumerable<PaletteEffectCollection> effects, string file)
        {
            if (effects == null || effects.Count() == 0)
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
            foreach (PaletteEffectCollection effect in effects)
            {
                filtered = effect.Effects.Where(x => x.EffectType != EffectType.None);
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
