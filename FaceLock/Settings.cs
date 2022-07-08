using System.IO;
using System.Xml.Serialization;
using UnityModManagerNet;

namespace FaceLock
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            var filepath = Path.Combine(modEntry.Path, "Settings.xml");
            using (var writer = new StreamWriter(filepath))
                new XmlSerializer(GetType()).Serialize(writer, this);
        }

        public void OnChange()
        {
        }

        public int faceIndex = -1;
    }
}