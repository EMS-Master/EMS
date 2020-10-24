using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineContracts
{
    [ServiceContract]
    public interface ICalculationEngineUIContract
    {
        [OperationContract]
        List<Tuple<double, DateTime>> GetHistoryMeasurements(long gid, DateTime startTime, DateTime endTime);

        [OperationContract]
        List<Tuple<double, DateTime>> GetTotalProduction(DateTime startTime, DateTime endTime);

        [OperationContract]
        List<Tuple<double, DateTime>> GetProfit(DateTime startTime, DateTime endTime);

        [OperationContract]
        List<Tuple<double, DateTime>> GetCost(DateTime startTime, DateTime endTime);
        

        [OperationContract]
        List<Tuple<double, DateTime>> GetCoReduction(DateTime startTime, DateTime endTime);

        [OperationContract]
        List<Tuple<double, DateTime>> GetCoEmission(DateTime startTime, DateTime endTime);

        [OperationContract]
        bool SetAlgorithmOptions(int iterationCount, int populationCount, int elitisamPct, float mutationRate);

        [OperationContract]
        bool SetAlgorithmOptionsDefault();

        [OperationContract]
        Tuple<int,int,int,float> GetAlgorithmOptions();

		[OperationContract]
		bool SetPricePerGeneratorType(float oilPrice, float coalPrice, float gasPrice);

		[OperationContract]
		bool SetPricePerGeneratorTypeDefault();

		[OperationContract]
		Tuple<float, float, float> GetPricePerGeneratorType();

		[OperationContract]
		void ResetCommandedGenerator(long gid);

		[OperationContract]
		List<float> GetPointForFuelEconomy(long gid);
	}
}
