using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonCloud.AzureStorage.Entities
{
    [DataContract]
    [Serializable()]
    public class TotalProduction : TableEntity
    {
        [DataMember]
        private int _id;
        [DataMember]
        private double _totalGeneration;
        [DataMember]
        private double _cO2Reduction;
        [DataMember]
        private double _cO2Emission;
        [DataMember]
        private double _totalCost;
        [DataMember]
        private double _profit;
        [DataMember]
        private DateTime _timeOfCalculation;

        public TotalProduction(double totalGeneration, double cO2Reduction, double cO2Emission, double totalCost, double profit, DateTime timeOfCalculation)
        {
            Id = 1;
            TotalGeneration = totalGeneration;
            CO2Reduction = cO2Reduction;
            CO2Emission = cO2Emission;
            TotalCost = totalCost;
            Profit = profit;
            TimeOfCalculation = timeOfCalculation;
            RowKey = DateTime.Now.ToString("o");
            PartitionKey = "TotalProduction";
        }


        public TotalProduction()
        { }
        public int Id { get => _id; set => _id = value; }
        public double TotalGeneration { get => _totalGeneration; set => _totalGeneration = value; }
        public double CO2Reduction { get => _cO2Reduction; set => _cO2Reduction = value; }
        public double CO2Emission { get => _cO2Emission; set => _cO2Emission = value; }
        public double TotalCost { get => _totalCost; set => _totalCost = value; }
        public double Profit { get => _profit; set => _profit = value; }
        public DateTime TimeOfCalculation { get => _timeOfCalculation; set => _timeOfCalculation = value; }


    }
}
