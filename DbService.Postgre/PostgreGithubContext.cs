using ClassScraper.DomainObjects.API;
using ClassScraper.DomainObjects.Github;
using DbLayer.PostgreService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassScraper.DbLayer.PostgreService
{
    public class PostgreGithubContext : DbContext, IGithubContext
    {
        const int PER_PAGE = 5;
        static PostgreGithubContext()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<TermType>();
        }
        public PostgreGithubContext(DbContextOptions<PostgreGithubContext> options) : base(options)
        {
            this.Database.SetCommandTimeout(10);
        }

        DbSet<PostgreFileEntity> Files { get; set; }
        DbSet<PostgreTermEntity> Terms { get; set; }

        public async Task<FileEntity> GetFileAsync(SearchModel search, CancellationToken ctoken = default)
        {
            FileEntity result = null;
            var q = BuildFileWhereClause(search)
                .Join(Terms, f => f.ID, t => t.PostgreFileEntityId, (File, Term) => new { File, Term });

            var list = await q.ToListAsync(ctoken);
            var terms = list.Select(s => s.Term);
            if (list.Count > 0)
            {
                var file = list.Select(f => f.File).First();
                file.Terms = terms;

                result = file.ToDomain(false);
            }

            return result;

        }
        public async Task<TermEntity> GetTermAsync(SearchModel search, CancellationToken ctoken = default)
        {
            TermEntity result = null;
            var t = await BuildTermWhereClause(search).Include(t => t.PostgreFileEntity).FirstOrDefaultAsync(ctoken);
            if (t != null)
            {
                result = t.ToDomain();
            }
            return result;
        }

        public async Task<ListSearchResult<FileEntity>> GetFilesAsync(SearchModel search, CancellationToken ctoken = default)
        {
            var q = BuildFileWhereClause(search);
            var count = await q.CountAsync();
            var query = await q
                .Skip(search.Page * PER_PAGE)
                .Take(PER_PAGE)
                .ToListAsync(ctoken);

            var result = new ListSearchResult<FileEntity>()
            {
                Data = query.Select(f => f.ToDomain()),
                TotalCount = count
            };

            return result;
        }

        public async Task<ListSearchResult<TermEntity>> GetTermsAsync(SearchModel search, CancellationToken ctoken = default)
        {
            var q = BuildTermWhereClause(search)
                .Include(p => p.PostgreFileEntity);

            var count = await q.CountAsync();
            var query = await q            
                .Skip(search.Page * PER_PAGE)
                .Take(PER_PAGE)
                .ToListAsync(ctoken);

            var result = new ListSearchResult<TermEntity>()
            {
                Data = query.Select(r => r.ToDomain()),
                TotalCount = count
            };

            return result;
        }

        public async Task<bool> SaveTermsAsync(FileEntity file, IEnumerable<TermEntity> terms, CancellationToken ctoken = default)
        {
            Files.Add(new PostgreFileEntity(file));
            Terms.AddRange(terms.Select(t => new PostgreTermEntity(t)));
            await this.SaveChangesAsync(ctoken);

            return await Task.FromResult(true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<TermType>();
        }

        private IQueryable<PostgreFileEntity> BuildFileWhereClause(SearchModel search)
        {
            var q = this.Set<PostgreFileEntity>().AsQueryable();
            if (search.FilterId != null)
            {
                q = q.Where(f => f.ID == search.FilterId);
            }
            if (search.FilterName != null)
            {
                q = q.Where(f => EF.Functions.ILike(f.Name, $"%{search.FilterName}%"));
            }

            return q;
        }
        private IQueryable<PostgreTermEntity> BuildTermWhereClause(SearchModel search)
        {
            var q = this.Set<PostgreTermEntity>().AsQueryable();
            if (search.FilterId != null)
            {
                q = q.Where(f => f.ID == search.FilterId);
            }
            if (search.FilterName != null)
            {
                q = q.Where(f => EF.Functions.ILike(f.Name, $"%{search.FilterName}%"));
            }
            if (search.FilterFileId != null)
            {
                q = q.Where(f => f.PostgreFileEntityId == search.FilterFileId);
            }
            if (search.TermType != TermType.Undefined)
            {
                q = q.Where(f => f.TermType == search.TermType);
            }

            return q;
        }

#if migration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=localhost;User Id=postgres;Password=1234;Port=5432;Database=scraper;Include Error Detail=true;");
        }
#endif
    }
    public static class ServiceCollectionExtensions
    {
        public static void AddPostgreSql(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IGithubContext, PostgreGithubContext>(_ =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<PostgreGithubContext>();
                optionsBuilder.UseNpgsql(configuration.GetConnectionString("Postgre"));
                optionsBuilder.EnableSensitiveDataLogging();
                return new PostgreGithubContext(optionsBuilder.Options);
            });
        }
    }

    public class GithubContextFactory : IDesignTimeDbContextFactory<PostgreGithubContext>
    {
        public PostgreGithubContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PostgreGithubContext>();
            optionsBuilder.UseNpgsql("Server=localhost;User Id=postgres;Password=1234;Port=5432;Database=Scraper;");

            return new PostgreGithubContext(optionsBuilder.Options);
        }
    }
}
