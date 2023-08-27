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
                var test = excelData.Sheets[3].Data;

                var cells = new Dictionary<string, object>();

                foreach (var sheet in excelData.Sheets)
                {
                    for (int i = 0; i < sheet.Data.Count; i++)
                    {
                        for (int j = 0; j < sheet.Data[i].Length; j++)
                        {
                            var obj = sheet.Data[i][j];
                            string num = (i + 1).ToString();
                            var letter = Enum.Parse<EnumAlphabet>(j.ToString());
                            var cell = letter + num;

                            cells.Add(cell, obj);
                        }
                    }
                }

                //for (int i = 0; i < test.Count; i++)
                //{
                //    for (int j = 0; j < test[i].Length; j++)
                //    {
                //        var obj = test[i][j];
                //        string num = (i + 1).ToString();
                //        var letter = Enum.Parse<EnumAlphabet>(j.ToString());
                //        var cell = letter + num;

                //        cells.Add(cell, obj);
                //    }
                //}

                var results = Calculate(cells);
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

        public static List<string[]> Calculate(Dictionary<string, object> cells)
        {
            Regex regex = new Regex(@"\=([A-Z]+)\((.*)\)");
            string operation = string.Empty;
            string[] operands = Array.Empty<string>();
            var values = new List<object>();
            var calculated = new List<string[]>();
            long calculation = 1;

            foreach (var cell in cells)
            {
                var match = regex.Match(cell.ToString());

                if (match.Success)
                {
                    operation = match.Groups[1].Value;
                    operands = match.Groups[2].Value.Split(", ");
                }
            }

            if (Enum.TryParse(operation, out EnumOperations result))
            {
                var type = cells[operands[0]].GetType();

                for (int i = 0; i < operands.Length; i++)
                {
                    if (type != cells[operands[i]].GetType())
                    {
                        Console.WriteLine("#ERROR: Incompatible types");
                    }

                    values.Add(cells[operands[i]]);
                }

                calculated.Add(operands);

                if (operation == "SUM")
                {
                    foreach (var item in values)
                    {
                        calculation += (Int64)item;
                    }
                }

                if (operation == "MULTIPLY")
                {
                    foreach (var item in values)
                    {
                        calculation *= (Int64)item;
                    }
                }

                calculated.Add(new[] { calculation.ToString() });
            }

            

            return calculated;
        }
    }
}
