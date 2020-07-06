using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class AlarmContext : DbContext
    {
        //public AlarmContext() : base("HistoryDB") { }
        public DbSet<Alarm> Alarms { get; set; }
    }
}
