using CalculationEngineContracts;
using CalculationEngineServ.DataBaseModels;
using CommonMeas;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    public class ProcessingToCalculation : ICalculationEngineContract, ICalculationEngineRepository
    {
        private static CalculationEngine ce = null;
        private readonly EmsContext _context = new EmsContext();

        public ProcessingToCalculation()
        {
           
        }

        public static CalculationEngine CalculationEngine { get => ce; set => ce = value; }
        
        public bool OptimisationAlgorithm(List<MeasurementUnit> measBatteryStorage, List<MeasurementUnit> measGenerators)
        {
            
            bool retVal = false;
            try
            {
                retVal = CalculationEngine.Optimize(measBatteryStorage, measGenerators);
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
            var list =  _context.DiscreteCounters.ToList();
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
                _context.DiscreteCounters.Add(new DiscreteCounter() { Gid = model.Gid, Counter = model.Counter, CurrentValue = model.CurrentValue });
            }
            else
            {
                var dc = _context.DiscreteCounters.FirstOrDefault(x => x.Id == model.Id);
                dc.CurrentValue = model.CurrentValue;
                dc.Counter = model.Counter;
                _context.Entry(dc).State = System.Data.Entity.EntityState.Modified;
            }

            _context.SaveChanges();
        }
    }
}
