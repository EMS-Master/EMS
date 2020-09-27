using CalculationEngineServ.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    public class EmsContext : DbContext
    {
        public DbSet<Alarm> Alarms { get; set; }
        public DbSet<DiscreteCounter> DiscreteCounters { get; set; }
        public DbSet<HistoryMeasurement> HistoryMeasurements { get; set; }
        public DbSet<TotalProduction> TotalProductions { get; set; }
        public DbSet<CommandedGenerator> CommandedGenerators { get; set; }
    }
}
