using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitStat.Core.Entities;


namespace GitStat.Core.Contracts
{
    public interface IDeveloperRepository
    {
        IEnumerable<Developer> GetDevelopersWithCommits();

        IEnumerable<Developer> GetAll();

        IEnumerable<Tuple<string, int, int, int, int>> GetDeveloperStatistics();
    }
}
