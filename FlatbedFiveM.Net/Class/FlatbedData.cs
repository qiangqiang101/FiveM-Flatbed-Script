namespace FlatbedFiveM.Net.Class
{
   public class FlatbedData
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
