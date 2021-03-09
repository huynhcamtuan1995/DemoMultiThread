using Newtonsoft.Json;
using System;

namespace ProcessThread
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
