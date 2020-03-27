using System;
using System.Collections.Generic;
using System.Linq;
using GitStat.Core.Contracts;
using GitStat.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GitStat.Persistence
{
    public class CommitRepository : ICommitRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CommitRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddRange(Commit[] commits)
        {
            _dbContext.Commits.AddRange(commits);
        }

        public IEnumerable<Commit> GetAll() => _dbContext.Commits
                                                    .OrderByDescending(c => c.Date)
                                                    .ToList();
        public Commit GetCommitById(int id) => _dbContext.Commits.Find(id);

        public IEnumerable<Commit> GetCommitsForTimePeriod(DateTime lower, DateTime upper) => _dbContext.Commits.Include(c => c.Developer)
                                                                                                                .Where(c => lower <= c.Date && c.Date <= upper)
                                                                                                                .OrderBy(_ => _.Date)
                                                                                                                .ToList();

        public Commit GetLatestCommit() => GetAll().First();
    }
}