using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitStat.Core.Entities;
using Utils;

namespace GitStat.ImportConsole
{
    public class ImportController
    {
        const string Filename = "commits.txt";

        /// <summary>
        /// Liefert die Messwerte mit den dazugehörigen Sensoren
        /// </summary>
        public static Commit[] ReadFromCsv()
        {
            string path = MyFile.GetFullNameInApplicationTree(Filename);
            string[] lines = File.ReadAllLines(path);
            List<Commit> result = new List<Commit>();
            Dictionary<string, Developer> devs = new Dictionary<string, Developer>();

            foreach (var line in lines)
            {
                if(!String.IsNullOrEmpty(line))
                {
                    if (!char.IsWhiteSpace(line[0]))
                    {
                        string[] data = line.Split(",");
                        DateTime time;
                        string message = String.Empty;
                        string hashCode = data[0];
                        string devName = data[1];

                        for (int i = 3; i < data.Length; i++)
                        {
                            message += $"{data[i]}";
                        }

                        DateTime.TryParse(data[2], out time);

                        if (!devs.ContainsKey(devName))
                        {
                            devs.Add(devName, new Developer()
                            {
                                Name = devName,
                                Commits = new List<Commit>()
                            });
                        }

                        Commit tmp = new Commit()
                        {
                            Developer = devs[devName],
                            Date = time,
                            HashCode = hashCode,
                            Message = message
                        };

                        result.Add(tmp);
                        result.Last().Developer.Commits.Add(tmp);
                    }
                    else if (char.IsWhiteSpace(line[0]) && line.Contains(","))
                    {
                        string[] data = line.Split(",");
                        int fileChanges = 0;
                        int insertions = 0;
                        int deletions = 0;

                        for (int i = 0; i < data.Length; i++)
                        {
                            string number = String.Empty;
                            int num = 0;
                            data[i] = data[i].TrimStart(' ');

                            for (int j = 0; j < data[i].Length; j++)
                            {
                                number += $"{data[i][j]}";

                                if (char.IsWhiteSpace(data[i][j]))
                                {
                                    num = Convert.ToInt32(number);
                                    j = data[i].Length;
                                }
                            }

                            switch (i)
                            {
                                case 0:
                                    fileChanges = num;
                                    break;
                                case 1:
                                    insertions = num;
                                    break;
                                case 2:
                                    deletions = num;
                                    break;
                                default:
                                    break;
                            }
                        }

                        result.Last().FilesChanges = fileChanges;
                        result.Last().Insertions = insertions;
                        result.Last().Deletions = deletions;
                    }
                }
            }

            return result.ToArray();
        }
    }
}
