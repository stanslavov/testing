using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                var endpoint = new Uri("https://www.wix.com/_serverless/hiring-task-spreadsheet-evaluator/sheets");
                var result = client.GetAsync(endpoint).Result;
                var json = result.Content.ReadAsStringAsync().Result;
                //Console.WriteLine(json);

                var excelData = JsonConvert.DeserializeObject<ExcelSheet>(json);
                var test = excelData.Sheets[7].Data;

                Console.WriteLine(test);

                //Type myType = test.GetType();
                //IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

                //foreach (PropertyInfo prop in props)
                //{
                //    object propValue = prop.GetValue(test, null);

                //    // Do something with propValue

                //    Console.WriteLine(propValue);

                // }
            }
        }
    }
}
