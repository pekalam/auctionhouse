﻿using Microsoft.EntityFrameworkCore;

namespace Adapter.SqlServer.EventOutbox
{
    internal class EventOutboxDbContext : DbContext
    {
        public DbSet<DbOutboxItem> OutboxItems { get; set; } = null!;

        public EventOutboxDbContext()
        {
        }

        public EventOutboxDbContext(DbContextOptions<EventOutboxDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DbOutboxItem>()
                    .OwnsOne(m => m.CommandContext);
            modelBuilder.Entity<DbOutboxItem>()
                  .OwnsOne(m => m.CommandContext);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
#if TEST
                
            optionsBuilder.UseSqlServer(System.Environment.GetEnvironmentVariable("ConnectionString"));
#endif
        }
    }
}