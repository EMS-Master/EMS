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
        private float _totalGeneration;
        [DataMember]
        private float _cO2Reduction;
        [DataMember]
        private float _cO2Emission;
        [DataMember]
        private float _totalCost;
        [DataMember]
        private float _profit;
        [DataMember]
        private DateTime _timeOfCalculation;

        public TotalProduction(int id, float totalGeneration, float cO2Reduction, float cO2Emission, float totalCost, float profit, DateTime timeOfCalculation)
        {
            Id = id;
            TotalGeneration = totalGeneration;
            CO2Reduction = cO2Reduction;
            CO2Emission = cO2Emission;
            TotalCost = totalCost;
            Profit = profit;
            TimeOfCalculation = timeOfCalculation;
            RowKey = Id.ToString();
            PartitionKey = "TotalProduction";
            Timestamp = DateTime.Now;
        }


        public TotalProduction()
        { }
        public int Id { get => _id; set => _id = value; }
        public float TotalGeneration { get => _totalGeneration; set => _totalGeneration = value; }
        public float CO2Reduction { get => _cO2Reduction; set => _cO2Reduction = value; }
        public float CO2Emission { get => _cO2Emission; set => _cO2Emission = value; }
        public float TotalCost { get => _totalCost; set => _totalCost = value; }
        public float Profit { get => _profit; set => _profit = value; }
        public DateTime TimeOfCalculation { get => _timeOfCalculation; set => _timeOfCalculation = value; }


    }
}
