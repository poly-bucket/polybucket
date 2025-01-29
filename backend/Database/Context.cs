using Database.Maps.Users;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        public static string ConnectionString =>
            $"Host={Environment.GetEnvironmentVariable("DB_HOST")};"
            + $"Port={Environment.GetEnvironmentVariable("DB_PORT")};"
            + $"Username={Environment.GetEnvironmentVariable("DB_USER")};"
            + $"Password={Environment.GetEnvironmentVariable("DB_PASS")};"
            + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
            + $"SSL Mode=none;";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users
            modelBuilder.ApplyConfiguration<Core.Models.Users.User>(new UserMap());
            modelBuilder.ApplyConfiguration<Core.Models.Users.UserLogin>(new UserLoginMap());

            // Models
            modelBuilder.ApplyConfiguration<Core.Models.Models.Model>(new ModelMap());
            modelBuilder.ApplyConfiguration<Core.Models.Models.PrintSettings>(new PrintSettingsMap());

            // Files
            modelBuilder.ApplyConfiguration<Core.Models.Files.File>(new FileMap());

            // Printers and Filaments
            modelBuilder.ApplyConfiguration<Core.Models.Printers.Printer>(new PrinterMap());
            modelBuilder.ApplyConfiguration<Core.Models.Filaments.Filament>(new FilamentMap());

            // Comments
            modelBuilder.ApplyConfiguration<Core.Models.Comments.Comment>(new CommentMap());
        }
    }
}