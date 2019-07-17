using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace FlatbedFiveM.Net.Class
{
    public class VehicleData
    {
        public VehicleData Instance
        {
            get
            {
                return ReadFromFile();
            }
        }

        [XmlIgnore]
        public string FileName { get; set; }

        public List<FlatbedData> Flatbeds;

        public VehicleData(string _fileName)
        {
            FileName = _fileName;
        }

        public void Save()
        {
            var ser = new XmlSerializer(typeof(VehicleData));
            TextWriter writer = new StreamWriter(FileName);
            ser.Serialize(writer, this);
            writer.Close();
        }

        public VehicleData ReadFromFile()
        {
            if (!File.Exists(FileName))
                return new VehicleData(FileName);

            try
            {
                var ser = new XmlSerializer(typeof(VehicleData));
                TextReader reader = new StreamReader(FileName);
                var instance = (VehicleData)ser.Deserialize(reader);
                reader.Close();
                return instance;
            }
            catch
            {
                return new VehicleData(FileName);
            }
        }
    }
}
