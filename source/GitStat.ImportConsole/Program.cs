using System;
using System.IO;
using System.Linq;
using System.Text;
using GitStat.Core.Contracts;
using GitStat.Persistence;
using Utils;

namespace GitStat.ImportConsole
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Import der Commits in die Datenbank");
            using (IUnitOfWork unitOfWorkImport = new UnitOfWork())
            {
                Console.WriteLine("Datenbank löschen");
                unitOfWorkImport.DeleteDatabase();
                Console.WriteLine("Datenbank migrieren");
                unitOfWorkImport.MigrateDatabase();
                Console.WriteLine("Commits werden von commits.txt eingelesen");
                var commits = ImportController.ReadFromCsv();
                //var commits = ImportController.ReadFromTxt();
                if (commits.Length == 0)
                {
                    Console.WriteLine("!!! Es wurden keine Commits eingelesen");
                    return;
                }
                Console.WriteLine(
                    $"  Es wurden {commits.Count()} Commits eingelesen, werden in Datenbank gespeichert ...");
                unitOfWorkImport.CommitRepository.AddRange(commits);
                int countDevelopers = commits.GroupBy(c => c.Developer).Count();
                int savedRows = unitOfWorkImport.SaveChanges();
                Console.WriteLine(
                    $"{countDevelopers} Developers und {savedRows - countDevelopers} Commits wurden in Datenbank gespeichert!");
                Console.WriteLine();
                /*var csvCommits = commits.Select(c =>
                    $"{c.Developer.Name};{c.Date};{c.Message};{c.HashCode};{c.FilesChanges};{c.Insertions};{c.Deletions}"); //Auskommentiert für Testzwecke bezüglich der .csv und der .txt Dateien
                File.WriteAllLines("commits.csv", csvCommits, Encoding.UTF8);*/                                             //da diese unterschiedliche Daten beinhalten
            }
            Console.WriteLine("Datenbankabfragen");
            Console.WriteLine("=================");
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                MyLogger.InitializeLogger();
                Console.WriteLine("Commits der letzten 4 Wochen ab dem letzten Commit");
                Console.WriteLine("--------------------------------------------------");

                DateTime lastCommit = unitOfWork.CommitRepository.GetLatestCommit().Date;
                DateTime lowerBound = lastCommit.AddDays(-28);

                var commits = unitOfWork.CommitRepository.GetCommitsForTimePeriod(lowerBound, lastCommit);
                                                        
                Console.WriteLine($"{"Developer", -18}{"Date", -10} FileChanges Insertions  Deletions ");
                foreach (var item in commits)
                {
                    Console.WriteLine($"{item.Developer.Name, -18}{item.Date.Date.ToShortDateString(), 10} {item.FilesChanges, 11} {item.Insertions, 10} {item.Deletions, 10} ");
                }

                Console.WriteLine("");
                Console.WriteLine("Commit mit Id 4");
                Console.WriteLine("---------------");

                var commit = unitOfWork.CommitRepository.GetCommitById(4);
                Console.WriteLine($"{commit.Developer.Name, -18}{commit.Date.Date.ToShortDateString(),10} {commit.FilesChanges,11} {commit.Insertions,10} {commit.Deletions,10} ");
                Console.WriteLine("");
                Console.WriteLine("Statistik der Commits der Developer");
                Console.WriteLine("-----------------------------------");

                var statistics = unitOfWork.DeveloperRepository.GetDeveloperStatistics();

                Console.WriteLine($"{"Developer", -18}{"Commits", 10} {"FileChanges", 11} {"Insertions", 10}  {"Deletions", 10}");
                foreach (var stat in statistics)
                {
                    Console.WriteLine($"{stat.Item1, -18}{stat.Item2, 10} {stat.Item3, 11} {stat.Item4, 10} {stat.Item5, 11}");
                }
            }
            Console.Write("Beenden mit Eingabetaste ...");
            Console.ReadLine();
        }
    }
}
