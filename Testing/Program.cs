using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

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
                var test = excelData.Sheets[8].Data;

                var line = new List<object>();

                for (int i = 0; i < test.Count; i++)
                {
                    for (int j = 0; j < test[i].Length; j++)
                    {
                        var obj = test[i][j];

                        line.Add(obj);
                    }
                }

                var results = Calculate(line);
                //var ress = new List<string[]>();
                //ress.Add(new[] { "22", "212212", "212234" });

                //var newPost = new ResultFile()
                //{
                //    Email = "someemail",
                //    Results = new[] { new ResultSheet { Data = ress } }
                //};

                //var newPostJson = JsonConvert.SerializeObject(newPost);
                //var payload = new StringContent(newPostJson, Encoding.UTF8, "application/json");
                //var res = client.PostAsync(endpoint, payload).Result.Content.ReadAsStringAsync().Result;
            }
        }

        public static List<string[]> Calculate(List<object> line)
        {
            Regex regex = new Regex(@"=[A-Z][A-Z]");
            var operands = new Queue<object>();
            long result = 0;
            var calculated = new List<string[]>();

            foreach (var item in line)
            {
                if (item.GetType() == typeof(Int64))
                {
                    calculated.Add(new[] { item.ToString() });
                    operands.Enqueue(item);
                }

                if (item.GetType() == typeof(bool))
                {
                    operands.Enqueue(item);
                }

                if (item.GetType() == typeof(string))
                {
                    var match = regex.Match(item.ToString());

                    if (match.ToString() == "=SU")
                    {
                        foreach (var operand in operands)
                        {
                            result += (Int64)operand;
                        }

                        calculated.Add(new[] { result.ToString() });
                    }

                    if (match.ToString() == "=MU")
                    {
                        foreach (var operand in operands)
                        {
                            result *= (Int64)operand;
                        }

                        calculated.Add(new[] { result.ToString() });
                    }

                    if (match.ToString() == "=DI")
                    {
                        result = (Int64)operands.Dequeue() / (Int64)operands.Peek();

                        calculated.Add(new[] { result.ToString() });
                    }
                }
            }

            return calculated;
        }
    }
}
