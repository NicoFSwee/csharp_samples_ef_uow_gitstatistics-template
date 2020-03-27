using System;
using System.Collections.Generic;
using System.Linq;
using GitStat.Core.Contracts;
using GitStat.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GitStat.Persistence
{
    public class DeveloperRepository : IDeveloperRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DeveloperRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Developer> GetAll() => _dbContext.Developers.ToList();

        public IEnumerable<Developer> GetDevelopersWithCommits() => _dbContext.Developers.Include(d => d.Commits);

        public IEnumerable<Tuple<string, int, int, int, int>> GetDeveloperStatistics() =>
            _dbContext.Developers.Select(_ => new Tuple<string, int, int, int, int>
                                (_.Name, _.Commits.Count(), _.Commits.Sum(s => s.FilesChanges), _.Commits.Sum(s => s.Insertions), _.Commits.Sum(s => s.Deletions)))
                                .ToList()
                                .OrderByDescending(_ => _.Item2);
    }
}