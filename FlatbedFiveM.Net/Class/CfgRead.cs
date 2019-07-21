//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FlatbedFiveM.Net.Class
//{
//    static class CfgRead
//    {
//        public static string ReadCfgValue(string key, string file)
//        {
//            string[] lines = System.IO.File.ReadAllLines(file);
//            string temp = null;
//            string value = null;

//            foreach (string l in lines)
//            {
//                if (l.StartsWith(key))
//                {
//                    temp = l.Substring(key.Length + 1);
//                    value = temp.Replace("\"", "");
//                    break;
//                }
//            }
//            return value;
//        }

//        public static void WriteCfgValue(string key, string value, string file__1)
//        {
//            string getext = file__1.Substring(file__1.LastIndexOf('.'));
//            string tmp = file__1.Replace(getext, ".tmp");
//            using (var sr = new System.IO.StreamReader(file__1))
//            {
//                using (var wr = new System.IO.StreamWriter(tmp))
//                {
//                    string line;
//                    bool check = false;
//                    do
//                    {
//                        line = sr.ReadLine();
//                        if (line == null)
//                            break;
//                        if (line.StartsWith(key))
//                        {
//                            line = string.Format("{0} \"{1}\"", key, value);
//                            check = true;
//                        }

//                        wr.WriteLine(line);
//                    }
//                    while (true)// Check if line is null then Exit Loop
//    ;
//                    if (!check)
//                        wr.WriteLine(string.Format("{0} \"{1}\"", key, value));
//                    sr.Close();
//                    wr.Close();
//                }
//            }
//            System.IO.File.Delete(file__1);
//            System.IO.File.Move(tmp, file__1);
//        }
//    }
//}
