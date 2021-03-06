using CalculationEngineContracts;
using CalculationEngineServ;
using CommonCloud.AzureStorage;
using CommonCloud.AzureStorage.Entities;
using CommonMeas;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineCloudServ
{
    public class ProcessingToCalculationCloud : ICalculationEngineContract, ICalculationEngineRepository
    {
        private static CalculationEngineCloud ce = null;

        public ProcessingToCalculationCloud()
        {

        }

        public static CalculationEngineCloud CalculationEngine { get => ce; set => ce = value; }

        public bool OptimisationAlgorithm(List<MeasurementUnit> measEnergyConsumer, List<MeasurementUnit> measGenerators, float windData, float sunData)
        {

            bool retVal = false;
            try
            {
                retVal = CalculationEngine.Optimize(measEnergyConsumer, measGenerators, windData, sunData);
            }
            catch (Exception ex)
            {
                string message = string.Format("Error: {0}", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }

            return retVal;
        }

        public List<DiscreteCounterModel> GetAllDiscreteCounters()
        {
            //var list =  DbManager.Instance.GetDiscreteCounters().ToList();
            //CommonCloud.AzureStorage.Entities.DiscreteCounter d = new CommonCloud.AzureStorage.Entities.DiscreteCounter(2,23456,true,3,"naziv2");

            //AzureTableStorage.AddTableEntityInDB(d,"UseDevelopmentStorage=true;", "DiscreteCounters");
            var list = AzureTableStorage.GetAllDiscreteCounters("UseDevelopmentStorage=true;", "DiscreteCounters");
            List<DiscreteCounterModel> returnList = new List<DiscreteCounterModel>();

            returnList = list.Select(x => new DiscreteCounterModel()
            {
                Id = x.Id,
                Gid = x.Gid,
                Counter = x.Counter,
                CurrentValue = x.CurrentValue
            }).ToList();

            return returnList;
        }

        public void InsertOrUpdate(DiscreteCounterModel model)
        {
            var discreteFromDb = DbManager.Instance.GetDiscreteCounters().FirstOrDefault(x => x.Gid == model.Gid);

            if (discreteFromDb == null)
            {
                DbManager.Instance.AddDiscreteCounter(new DiscreteCounter(model.Gid, model.CurrentValue, model.Counter, model.Name));
            }
            else
            {
                discreteFromDb.CurrentValue = model.CurrentValue;
                discreteFromDb.Counter = model.Counter;
                DbManager.Instance.AddDiscreteCounter(discreteFromDb);
            }

            //DbManager.Instance.SaveChanges();
        }

        public Dictionary<Tuple<long, string>, int> GetCounterForGeneratorType()
        {
            return CalculationEngineCloud.MaxDiscreteCounter;
        }
    }
}
