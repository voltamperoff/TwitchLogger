using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TwitchLogger.Models.Database;

namespace TwitchLogger
{
    internal class Context : DbContext
    {
        private readonly string path;

        public Context(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "Database path must be specified");
            }

            this.path = path;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var stringBuilder = new SqliteConnectionStringBuilder() { DataSource = path };

            optionsBuilder.UseSqlite(stringBuilder.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Channel>().ToTable("channels");
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Message>().ToTable("messages");

            modelBuilder.Entity<Channel>().HasIndex(c => c.Name).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Name).IsUnique();
        }

        public DbSet<Channel> Channels { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
