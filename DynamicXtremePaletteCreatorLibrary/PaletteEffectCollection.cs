using System.Text.Json;

namespace DynamicXtremePaletteCreatorLibrary
{
    /// <summary>Represents an ordered effect collection with a runtime-only logical name.</summary>
    [Serializable]
    public class PaletteEffectCollection
    {
        /// <summary>
        /// Gets or sets the logical name assigned by the tool. It is excluded from JSON;
        /// legacy Name fields are ignored when loading palette-effect files.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
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
        /// <summary>Loads ordered effects while ignoring legacy Name fields and preserving the runtime name.</summary>
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
        /// <summary>Serializes effects as indented JSON without the runtime-only logical name.</summary>
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
