using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Console.WriteLine(json);

                var excelData = JsonConvert.DeserializeObject<ExcelFile>(json);
                var test = excelData.Sheets[3].Data;
                var operands = new Queue<object>();
                var operators = new Queue<object>();
                var booleans = new Queue<object>();


                for (int i = 0; i < test.Count; i++)
                {
                    for (int j = 0; j < test[i].Length; j++)
                    {
                        var obj = test[i][j];

                        if (obj.GetType() == typeof(Int64))
                        {
                            operands.Enqueue(obj);
                        }

                        if (obj.GetType() == typeof(string))
                        {
                            operators.Enqueue(obj);
                        }

                        if (obj.GetType() == typeof(bool))
                        {
                            booleans.Enqueue(obj);
                        }
                    }
                }

                //Console.WriteLine(list);

                //Type myType = test.GetType();
                //IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

                //foreach (PropertyInfo prop in props)
                //{
                //    object propValue = prop.GetValue(test, null);

                //    // Do something with propValue

                //    Console.WriteLine(propValue);

                //}
            }
        }
    }
}
