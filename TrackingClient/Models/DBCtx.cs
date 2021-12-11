using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackingClient.Models;
using Microsoft.EntityFrameworkCore;

namespace TrackingClient.Models
{

    public partial class DBCtx : DbContext
    {
        public DBCtx(DbContextOptions<DBCtx> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<TagTest> TagTests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TagTest>().ToTable("TagTest");
        }
    }
}
