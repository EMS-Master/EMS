using CalculationEngineContracts;
using CalculationEngineServ.DataBaseModels;
using CommonMeas;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CalculationEngineServ
{
    public class ProcessingToCalculation : ICalculationEngineContract, ICalculationEngineRepository
    {
        private static CalculationEngine ce = null;

        public ProcessingToCalculation()
        {
           
        }

        public static CalculationEngine CalculationEngine { get => ce; set => ce = value; }
        
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
            var list =  DbManager.Instance.GetDiscreteCounters().ToList();
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
            if(model.Id == 0)
            {
               DbManager.Instance.AddDiscreteCounter(new DiscreteCounter() { Gid = model.Gid, Counter = model.Counter, CurrentValue = model.CurrentValue, Name = model.Name });
            }
            else
            {
                var dc = DbManager.Instance.GetDiscreteCounters().FirstOrDefault(x => x.Id == model.Id);
                dc.CurrentValue = model.CurrentValue;
                dc.Counter = model.Counter;
                DbManager.Instance.UpdateDiscreteCounter(dc);
            }

            DbManager.Instance.SaveChanges();
        }

        public Dictionary<Tuple<long, string>, int> GetCounterForGeneratorType()
        {
            return CalculationEngine.MaxDiscreteCounter;
        }
    }
}
