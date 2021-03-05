using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ConsoleThread
{
    public class Common
    {
        public static void WriteLog(object input)
        {
                string json = JsonConvert.SerializeObject(input);

            Console.WriteLine(json);
        }
    }
}
