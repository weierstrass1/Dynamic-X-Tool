using System.Text.Json;

namespace DynamicXtremePaletteCreatorLibrary
{
    /// <summary>Represents a named, JSON-serializable ordered collection of palette effects.</summary>
    [Serializable]
    public class PaletteEffectCollection
    {
        /// <summary>Gets or sets the logical collection name.</summary>
        public string? Name { get; set; }
        /// <summary>Gets or sets the ordered effects.</summary>
        public List<PaletteEffect> Effects { get; set; } = [];
        /// <summary>Gets the effect at a specified index.</summary>
        public PaletteEffect this[int index] => Effects[index];
        /// <summary>Initializes an empty effect collection.</summary>
        public PaletteEffectCollection()
        {
        }
        /// <summary>Appends an effect.</summary>
        public void Add(PaletteEffect effect) => Effects.Add(effect);
        /// <summary>Inserts an effect at a specific index.</summary>
        public void AddAt(PaletteEffect effect, int index) => Effects.Insert(index, effect);
        /// <summary>Removes the effect at a specific index.</summary>
        public void RemoveAt(int  index) => Effects.RemoveAt(index);
        /// <summary>Replaces the collection by deserializing a validated JSON file.</summary>
        public bool Load(string path)
        {
            string jsonContent = File.ReadAllText(path);
            try
            {
                var pec = JsonSerializer.Deserialize<PaletteEffectCollection>(jsonContent.Trim());
                if (pec == null || pec.Effects == null || pec.Effects.Any(effect => effect == null))
                    return false;
                Effects = pec.Effects;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>Serializes the collection as indented JSON.</summary>
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
