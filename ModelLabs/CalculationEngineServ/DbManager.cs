using CalculationEngineServ.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    public class DbManager
    {
        private static DbManager instance = null;

        public EmsContext emsContext = null;

        private DbManager()
        {
            emsContext = new EmsContext();
        }

        public static DbManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DbManager();
                }
                return instance;
            }
        }

        #region Alarms
        public IQueryable<Alarm> GetAlarms()
        {
            lock (emsContext)
            {
                return emsContext.Alarms;
            }
        }

        public void AddAlarm(Alarm alarm)
        {
            lock (emsContext)
            {
                emsContext.Alarms.Add(alarm);
            }
        }
        #endregion

        #region DiscreteCounters
        public IQueryable<DiscreteCounter> GetDiscreteCounters()
        {
            lock (emsContext)
            {
                return emsContext.DiscreteCounters;
            }
        }

        public void AddDiscreteCounter(DiscreteCounter dc)
        {
            lock (emsContext)
            {
                emsContext.DiscreteCounters.Add(dc);
            }
        }

        public void UpdateDiscreteCounter(DiscreteCounter dc)
        {
            emsContext.Entry(dc).State = System.Data.Entity.EntityState.Modified;
        }

        #endregion

        #region TotalProduction
        public IQueryable<TotalProduction> GetTotalProductions()
        {
            return emsContext.TotalProductions;
        }

        public void AddTotalProduction(TotalProduction tot)
        {
            emsContext.TotalProductions.Add(tot);
        }
        #endregion

        #region CO2Emission

        public void AddCO2Emission(CO2Emission ems)
        {
            emsContext.CO2Emissions.Add(ems);
        }

        #endregion

        #region HistoryMeasurements

        public IQueryable<HistoryMeasurement> GetHistoryMeasurements()
        {
            return emsContext.HistoryMeasurements;
        }

        public void AddHistoryMeasurement(HistoryMeasurement hm)
        {
            emsContext.HistoryMeasurements.Add(hm);
        }

        #endregion

        public void SaveChanges()
        {
            emsContext.SaveChanges();
        }
    }
}
