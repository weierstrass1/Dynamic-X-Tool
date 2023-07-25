using DynamicXPaletteCreatorLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicXLibrary
{
    public class PaletteEffectExtension
    {
        public static List<PaletteEffectCollection> GetCollections()
        {
            string[] paths = Directory.GetFiles("PaletteEffects", "*.paleffect");
            PaletteEffectCollection? collection;
            List<PaletteEffectCollection> res = new();
            foreach (var path in paths)
            {
                collection = new()
                {
                    Name = Path.GetFileNameWithoutExtension(path)
                };
                if (collection.Load(path))
                    res.Add(collection);
            }
            return res;
        }
        public static byte[] ToBin(List<PaletteEffectCollection> effects)
        {
            List<byte> type = new();
            List<byte> c1 = new();
            List<byte> c2 = new();
            List<byte> c3 = new();
            List<byte> r1 = new();
            List<byte> r2 = new();
            List<byte> r3 = new();
            int counter = 0;
            foreach (PaletteEffectCollection effect in effects)
            {
                type.AddRange(effect.Effects.Select(x => (byte)x.Channels));
                c1.AddRange(effect.Effects.Select(x => x.Channel1));
                c2.AddRange(effect.Effects.Select(x => x.Channel2));
                c3.AddRange(effect.Effects.Select(x => x.Channel3));
                r1.AddRange(effect.Effects.Select(x => x.Ratio1));
                r2.AddRange(effect.Effects.Select(x => x.Ratio2));
                r3.AddRange(effect.Effects.Select(x => x.Ratio3));
                counter += effect.Effects.Count;
            }
            List<byte> res = new()
            {
                (byte)counter,
                (byte)(counter / 256)
            };
            res.AddRange(type);
            res.AddRange(c1);
            res.AddRange(c2);
            res.AddRange(c3);
            res.AddRange(r1);
            res.AddRange(r2);
            res.AddRange(r3);
            return res.ToArray();
        }
    }
}
