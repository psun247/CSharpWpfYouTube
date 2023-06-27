using Microsoft.EntityFrameworkCore;
using CSharpWpfYouTube.Models;

namespace CSharpWpfYouTube.Services
{
    // SQLite EF Core        
    public class SQLiteDbContext : DbContext
    {
        private string _dbFilePath = string.Empty;

        public DbSet<SLAppSetting> SLAppSettings { get; set; }        
        public DbSet<SLVideoGroup> SLVideoGroups { get; set; }
        public DbSet<SLVideoInfo> SLVideoInfos { get; set; }

        public SQLiteDbContext(string dbFilePath)
        {
            _dbFilePath = dbFilePath;

            Database.EnsureCreated();
        }

        public SQLiteDbContext(DbContextOptions<SQLiteDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlite($"Data Source={_dbFilePath}");            
            optionsBuilder.EnableSensitiveDataLogging();
        }

        // SQLite holds some info in memory because this is only called once for multiple new SQLiteDbContext()
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SLAppSetting>(eb => eb.HasKey(x => x.Name));
            modelBuilder.Entity<SLVideoGroup>(eb => eb.HasKey(x => x.Id));
            modelBuilder.Entity<SLVideoInfo>(eb => eb.HasKey(x => x.Id));
            
            base.OnModelCreating(modelBuilder);
        }                      
    }
}
