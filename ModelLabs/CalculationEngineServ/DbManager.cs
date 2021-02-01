
using CommonCloud.AzureStorage;
using CommonCloud.AzureStorage.Entities;
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

        //public EmsContext emsContext = null;

        private DbManager()
        {
            //emsContext = new EmsContext();
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
        public List<Alarm> GetAlarms()
        {

            return AzureTableStorage.GetAllAlarms("UseDevelopmentStorage=true;", "Alarms");
            
        }

        public void AddAlarm(Alarm alarm)
        {

            AzureTableStorage.AddTableEntityInDB(alarm,"UseDevelopmentStorage=true;", "Alarms");

        }
        #endregion

        #region DiscreteCounters
        public List<DiscreteCounter> GetDiscreteCounters()
        {

            return AzureTableStorage.GetAllDiscreteCounters("UseDevelopmentStorage=true;", "DiscreteCounters");

        }

        public void AddDiscreteCounter(DiscreteCounter dc)
        {
            AzureTableStorage.AddTableEntityInDB(dc,"UseDevelopmentStorage=true;", "DiscreteCounters");

        }

        //public void UpdateDiscreteCounter(DiscreteCounter dc)
        //{
        //    emsContext.Entry(dc).State = System.Data.Entity.EntityState.Modified;
        //}

		#endregion

		#region CommandedGenerators
		public List<CommandedGenerator> GetCommandedGenerators()
		{
			
				return AzureTableStorage.GetAllCommandedGenerators("UseDevelopmentStorage=true;", "CommandedGenerators");

        }
		
		public CommandedGenerator GetCommandedGenerator(long gid)
		{
				return AzureTableStorage.GetAllCommandedGenerators("UseDevelopmentStorage=true;", "CommandedGenerators").FirstOrDefault(x => x.Gid == gid);
			
		}

		public void AddCommandedGenerator(CommandedGenerator cg)
		{
            AzureTableStorage.AddTableEntityInDB(cg, "UseDevelopmentStorage=true;", "CommandedGenerators");

        }

		public void AddListCommandedGenerators(List<CommandedGenerator> listCG)
		{
            AzureTableStorage.InsertEntitiesInDB(listCG, "UseDevelopmentStorage=true;", "CommandedGenerators");

        }

		//public void UpdateCommandedGenerator(CommandedGenerator cg)
		//{
		//	emsContext.Entry(cg).State = System.Data.Entity.EntityState.Modified;
		//}

		#endregion

		#region TotalProduction
		public List<TotalProduction> GetTotalProductions()
        {
            return AzureTableStorage.GetAllTotalProductions( "UseDevelopmentStorage=true;", "TotalProductions");
        }

        public void AddTotalProduction(TotalProduction tot)
        {
            AzureTableStorage.AddTableEntityInDB(tot, "UseDevelopmentStorage=true;", "TotalProductions");
        }
        #endregion

        #region HistoryMeasurements

        public List<HistoryMeasurement> GetHistoryMeasurements()
        {
            return AzureTableStorage.GetAllHistoryMeasurements("UseDevelopmentStorage=true;", "HistoryMeasurements");
        }

        public void AddHistoryMeasurement(HistoryMeasurement hm)
        {
            AzureTableStorage.AddTableEntityInDB(hm, "UseDevelopmentStorage=true;", "HistoryMeasurements");
        }

        #endregion

        //public void SaveChanges()
        //{
        //    emsContext.SaveChanges();
        //}
    }
}
