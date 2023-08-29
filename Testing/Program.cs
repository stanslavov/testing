﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
                var endpoint2 = new Uri("https://www.wix.com/_serverless/hiring-task-spreadsheet-evaluator/verify/eyJ0YWdzIjpbXX0");
                var result = client.GetAsync(endpoint).Result;
                var json = result.Content.ReadAsStringAsync().Result;
                //Console.WriteLine(json);

                var excelData = JsonConvert.DeserializeObject<ExcelFile>(json);
                //var test = excelData.Sheets[15].Data;

                var cells = new Dictionary<string, object>();
                var resultSheets = new List<ResultSheet>();
                var post = new ResultFile()
                {
                    Email = "stanislav.slavov88@gmail.com",
                    Results = resultSheets.ToArray()
                };

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

                    resultSheets.Add(new ResultSheet { Id = sheet.Id, Data = Calculate(cells) });

                    cells.Clear();
                }

                post.Results = resultSheets.ToArray();

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

                //var results = Calculate(cells);

                var newPostJson = JsonConvert.SerializeObject(post);
                //Console.WriteLine(newPostJson);
                var payload = new StringContent(newPostJson, Encoding.UTF8, "application/json");
                var res = client.PostAsync(endpoint2, payload).Result.Content.ReadAsStringAsync().Result;
                Console.WriteLine(res);
            }
        }

        public static List<string[]> Calculate(Dictionary<string, object> cells)
        {
            Regex regex = new Regex(@"\=([A-Z]+)\((.*)\)");
            Regex regex2 = new Regex(@"\=([A-Z]+)\(([A-Z]+)\(([A-Z][0-9]\,\s[A-Z][0-9])\)");
            Regex regex3 = new Regex(@"\=([A-Z]+)\(.([A-Z][a-z]+)..........([A-Z][a-z]+.).\)");
            Regex regex4 = new Regex(@"\=([A-Z]+)\(([A-Z][0-9])....([a-z]+)....([[A-Z][0-9])\)");
            Regex regex5 = new Regex(@"(=)([A-Z][0-9])");
            string operation = string.Empty;
            string[] operands = Array.Empty<string>();
            var values = new List<object>();
            var calculated = new List<string[]>();
            long calculation = 0;
            bool evaluation = false;
            string concatenation = string.Empty;

            for (int i = 0; i < cells.Count; i++)
            {
                calculated.Add(new string[cells.Count]);
            }
            
            foreach (var cell in cells)
            {
                var cellRow = int.Parse(cell.Key[1].ToString());
                calculated[cellRow - 1][cellRow - 1] = cell.Value.ToString();

                var match = regex.Match(cell.ToString());

                if (match.Success)
                {
                    operation = match.Groups[1].Value;
                    operands = match.Groups[2].Value.Split(", ");
                }

                var match2 = regex2.Match(cell.ToString());

                if (match2.Success)
                {
                    operation = match2.Groups[1].Value;
                    operands = match2.Groups[3].Value.Split(", ");
                }

                var match3 = regex3.Match(cell.ToString());

                if (match3.Success)
                {
                    operation = match3.Groups[1].Value;
                    operands = new[] { match3.Groups[2].Value + ", " + match3.Groups[3].Value };
                }

                var match4 = regex4.Match(cell.ToString());

                if (match4.Success)
                {
                    operation = match4.Groups[1].Value;
                    operands = new[] { cells[match4.Groups[2].Value] + " " + match4.Groups[3].Value + " " + cells[match4.Groups[4].Value] };
                }

                var match5 = regex5.Match(cell.ToString());

                if (match5.Success)
                {
                    operation = match5.Groups[1].Value;
                    var op = calculated.FirstOrDefault(x => x.First().Contains('='));
                    var ind = calculated.IndexOf(op);
                    calculated[cellRow - 1][cellRow - 1] = cells[match5.Groups[2].Value].ToString();
                }
            }

            if (Enum.TryParse(operation, out EnumOperations result))
            {
                if (operation == "CONCAT")
                {
                    for (int i = 0; i < operands.Length; i++)
                    {
                        values.Add(operands[i]);
                    }
                }

                else
                {
                    for (int i = 0; i < operands.Length; i++)
                    {
                        if (!cells.ContainsKey(operands[i]))
                        {
                            long parsed = 0;
                            Int64.TryParse(operands[i], out parsed);
                            values.Add(parsed);
                        }
                        else
                        {
                            values.Add(cells[operands[i]]);
                        }
                    }

                    for (int i = 0; i < values.Count; i++)
                    {
                        var type = values[0].GetType();

                        if (type != values[i].GetType())
                        {
                            values.Clear();
                            break;
                        }
                    }

                    //var type = cells[operands[0]].GetType();

                    //for (int i = 0; i < operands.Length; i++)
                    //{
                    //    if (!cells.ContainsKey(operands[i]) || type != cells[operands[i]].GetType())
                    //    {
                    //        Console.WriteLine("#ERROR: Incompatible types");
                    //        break;
                    //    }

                    //    values.Add(cells[operands[i]]);
                    //}
                }

                if (operation == "SUM")
                {
                    foreach (var item in values)
                    {
                        calculation += (Int64)item;
                    }
                }

                if (operation == "MULTIPLY")
                {
                    calculation++;

                    foreach (var item in values)
                    {
                        calculation *= (Int64)item;
                    }
                }

                if (operation == "DIVIDE")
                {
                    calculation = (Int64)values[0] / (Int64)values[1];
                }

                if (operation == "GT")
                {
                    if ((Int64)values[0] > (Int64)values[1])
                    {
                        evaluation = true;
                    }
                    else
                    {
                        evaluation = false;
                    }
                }

                if (operation == "EQ")
                {
                    if ((double)values[0] == (double)values[1])
                    {
                        evaluation = true;
                    }
                    else
                    {
                        evaluation = false;
                    }
                }

                if (operation == "NOT")
                {
                    foreach (var item in values)
                    {
                        evaluation = !(bool)item;
                    }
                }

                if (operation == "AND")
                {
                    foreach (var item in values)
                    {
                        if ((bool)item)
                        {
                            evaluation = true;
                        }
                        else
                        {
                            evaluation = false;
                            break;
                        }
                    }
                }

                if (operation == "OR")
                {
                    foreach (var item in values)
                    {
                        if ((bool)item)
                        {
                            evaluation = true;
                            break;
                        }
                    }
                }

                if (operation == "IF")
                {
                    if ((Int64)values[0] > (Int64)values[1])
                    {
                        calculation = (Int64)values[0];
                    }

                    else
                    {
                        calculation = (Int64)values[1];
                    }
                }

                if (operation == "CONCAT")
                {
                    foreach (var item in values)
                    {
                        concatenation += ((string)item);
                    }
                }

                var formula = calculated.FirstOrDefault(x => x.First().Contains('='));
                var index = calculated.IndexOf(formula);
                calculated.Remove(formula);

                if (calculation > 0)
                {
                    calculated.Insert(index, new[] { calculation.ToString() });
                }

                else if (!string.IsNullOrEmpty(concatenation))
                {
                    calculated.Insert(index, new[] { concatenation });
                }

                else if (!values.Any())
                {
                    calculated.Insert(index, new[] { cells[operands[0]].ToString(), cells[operands[1]].ToString(), "#ERROR: Incompatible types" });
                }

                else
                {
                    calculated.Insert(index, new[] { evaluation.ToString() });
                }
            }

            return calculated;
        }
    }
}
