using System.Text.Json;

namespace DynamicXtremePaletteCreatorLibrary
{
    [Serializable]
    public class PaletteEffectCollection
    {
        public string? Name { get; set; }
        public List<PaletteEffect> Effects { get; set; } = [];
        public PaletteEffect this[int index] => Effects[index];
        public PaletteEffectCollection()
        {
        }
        public void Add(PaletteEffect effect) => Effects.Add(effect);
        public void AddAt(PaletteEffect effect, int index) => Effects.Insert(index, effect);
        public void RemoveAt(int  index) => Effects.RemoveAt(index);
        public bool Load(string path)
        {
            string jsonContent = File.ReadAllText(path);
            try
            {
                var pec = JsonSerializer.Deserialize<PaletteEffectCollection>(jsonContent.Trim());
                if (pec == null)
                    return false;
                Effects = pec.Effects;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void Save(string path)
        {
            string json = JsonSerializer.Serialize(this,new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            if(File.Exists(path)) 
                File.Delete(path);
            File.WriteAllText(path, json);
        }
    }
}
