using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Xml;

namespace Database
{
    public class Context
    {
        public Context(DbContextOptions<Context> options) : base(options)
        { }

        public static string ConnectionString =>
            $"Host={Environment.GetEnvironmentVariable("DB_HOST")};"
            + $"Port={Environment.GetEnvironmentVariable("DB_PORT")};"
            + $"Username={Environment.GetEnvironmentVariable("DB_USER")};"
            + $"Password={Environment.GetEnvironmentVariable("DB_PASS")};"
            + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
            + $"SSL Mode=none;";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                ServerVersion version = ServerVersion.AutoDetect(ConnectionString);
                optionsBuilder.UseMySql(ConnectionString, version);
            }
        }
    }
}