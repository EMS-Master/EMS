﻿using CalculationEngineContracts;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    public class CeToUI : ICalculationEngineUIContract
    {
        private static CalculationEngine ce = null;
        public CeToUI()
        {
        }

        public static CalculationEngine Ce { set => ce = value; }

        public List<Tuple<double, DateTime>> GetHistoryMeasurements(long gid, DateTime startTime, DateTime endTime)
        {
            List<Tuple<double, DateTime>> retList = new List<Tuple<double, DateTime>>();
            try
            {
                retList = ce.ReadMeasurementsFromDb(gid, startTime, endTime);
            }
            catch (Exception ex)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "[CeToUI] Error GetHistoryMeasurements {0}", ex.Message);
            }

            return retList;
        }

    }
}