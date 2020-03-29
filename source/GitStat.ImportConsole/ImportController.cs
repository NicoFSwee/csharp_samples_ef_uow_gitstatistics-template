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
        const string FilenameCsv = "commits.csv";

        /// <summary>
        /// Liefert die Messwerte mit den dazugehörigen Sensoren
        /// </summary>
        public static Commit[] ReadFromCsv()
        {
            string path = MyFile.GetFullNameInApplicationTree(FilenameCsv);
            string[] lines = File.ReadAllLines(path);

            var devs = lines.Select(l => l.Split(";"))
                            .GroupBy(_ => _[0])
                            .Select(_ => new Developer() { Name = _.Key, Commits = new List<Commit>()})
                            .ToDictionary(_ => _.Name);

            var result = lines.Select(l => l.Split(";"))
                            .Select(_ => new Commit()
                            {
                                Developer = devs[_[0]],
                                Date = DateTime.Parse(_[1]),
                                Message = _[2],
                                HashCode = _[3],
                                FilesChanges = int.Parse(_[4]),
                                Insertions = int.Parse(_[5]),
                                Deletions = int.Parse(_[6])
                            })
                            .ToList();

            foreach (var item in result)
            {
                item.Developer.Commits.Add(item);
            }

            return result.ToArray();
        }

        public static Commit[] ReadFromTxt()
        {
            string path = MyFile.GetFullNameInApplicationTree(Filename);
            string[] lines = File.ReadAllLines(path);
            List<char> removeText = new List<char>()
            {
                'a', 'b', 'c','d', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '(', ')', '+', '-', '#', ','
            };

            var devs = lines.Where(l => !String.IsNullOrEmpty(l) && !Char.IsWhiteSpace(l[0]))
                            .Select(_ => _.Split(","))
                            .GroupBy(_ => _[1])
                            .Select(_ => new Developer() { Name = _.Key, Commits = new List<Commit>() })
                            .ToDictionary(_ => _.Name);

            var result = lines.Where(l => !String.IsNullOrEmpty(l) && !Char.IsWhiteSpace(l[0]))
                            .Select(_ => _.Split(","))
                            .Select(_ => new Commit()
                            {
                                HashCode = _[0],
                                Developer = devs[_[1]],
                                Date = DateTime.Parse(_[2]),
                                Message = _[3]
                            })
                            .ToList();

            foreach(var r in result)
            {
                string[] data = lines.SkipWhile(l => !l.Contains(r.HashCode))
                                    .SkipWhile(_ => !_.Contains("Merge") && !_.Contains("|"))
                                    .SkipWhile(l => l.Contains("|"))
                                    .Select(_ => _.Split(","))
                                    .FirstOrDefault();

                r.FilesChanges = data.SkipWhile(_ => !_.Contains("changed"))
                                    .Select(_ => _.Filter(removeText))
                                    .Select(_ => int.Parse(_))
                                    .FirstOrDefault();

                r.Insertions = data.SkipWhile(_ => !_.Contains("insertion"))
                                    .Select(_ => _.Filter(removeText))
                                    .Select(_ => int.Parse(_))
                                    .FirstOrDefault();

                r.Deletions = data.SkipWhile(_ => !_.Contains("deletion"))
                                    .Select(_ => _.Filter(removeText))
                                    .Select(_ => int.Parse(_))
                                    .FirstOrDefault();

                r.Developer.Commits.Add(r);
            }

            return result.ToArray();
        }
    }

    public static class Extension
    {
        public static string Filter(this string str, List<char> charsToRemove)
        {
            foreach (char c in charsToRemove)
            {
                str = str.Replace(c.ToString(), String.Empty);
            }

            return str;
        }
    }
}
