using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace FlatbedFiveMServer.net
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

    public struct FlatbedData
    {
        public string Model;
        public string AttachDummy;
        public string WinchDummy;
        public string ControlDummy;
        public string ControlDummy2;
        public bool ControlIsOutside;

        public FlatbedData(string m, string ad, string wd, string cd, string cd2, bool cio)
        {
            Model = m;
            AttachDummy = ad;
            WinchDummy = wd;
            ControlDummy = cd;
            ControlDummy2 = cd2;
            ControlIsOutside = cio;
        }
    }
}
