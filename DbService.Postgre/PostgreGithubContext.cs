﻿using ClassScraper.DomainObjects.API;
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

        public Task<FileEntity> GetFileAsync(string file_id, CancellationToken ctoken = default)
        {
            throw new NotImplementedException();
        }        

        public async Task<ListSearchResult<FileEntity>> GetFilesAsync(SearchModel search, CancellationToken ctoken = default)
        {
            var q = this.Set<PostgreFileEntity>()
                .Where(f => EF.Functions.ILike(f.Name, $"%{search.FilterName}%"));

            var query = await q                                
                .Select(p => new { T = p, Count = q.Count() })
                .Skip(search.Page * PER_PAGE)
                .Take(PER_PAGE)                
                .ToListAsync(ctoken);

            var result = new ListSearchResult<FileEntity>()
            {
                Data = query.Select(f => f.T.ToDomain()),
                TotalCount = query.FirstOrDefault()?.Count ?? 0
            };


            return result;
        }        

        public async Task<ListSearchResult<TermEntity>> GetTermsAsync(SearchModel search, CancellationToken ctoken = default)
        {
            var q = this.Set<PostgreTermEntity>()
                .Where(f => EF.Functions.ILike(f.Name, $"%{search.FilterName}%"))
                .Include(p => p.PostgreFileEntity);

            var query = await q                
                .Select(p => new { T = p, Count = q.Count() })
                .Skip(search.Page *  PER_PAGE)
                .Take(PER_PAGE)                
                .ToListAsync(ctoken);

            var result = new ListSearchResult<TermEntity>()
            {
                Data = query.Select(r => r.T.ToDomain()),
                TotalCount = query.FirstOrDefault()?.Count ?? 0
            };

            return result;
        }

        public async Task<bool> SaveTermsAsync(FileEntity file, IEnumerable<TermEntity> terms, CancellationToken cancellationToken = default)
        {
            Files.Add(new PostgreFileEntity(file));
            Terms.AddRange(terms.Select(t => new PostgreTermEntity(t)));
            await this.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<TermType>();
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
