using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sbt;

namespace Sbt.Data
{
    public class DemoContext : DbContext
    {
        public DemoContext (DbContextOptions<DemoContext> options)
            : base(options)
        {
        }

        public DbSet<Sbt.Divisions> Divisions { get; set; } = default!;
        public DbSet<Sbt.Schedules> Schedules { get; set; } = default!;
        public DbSet<Sbt.Standings> Standings { get; set; } = default!;
    }
}
