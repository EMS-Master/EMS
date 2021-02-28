using CommonCloud.AzureStorage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonCloud.AzureStorage.Helpers
{
    [DataContract]
    [Serializable()]
    public class TotalProductionHelper 
    {
        //[DataMember]
        private int _id;
        //[DataMember]
        private double _totalGeneration;
        //[DataMember]
        private double _cO2Reduction;
        //[DataMember]
        private double _cO2Emission;
        //[DataMember]
        private double _totalCost;
        //[DataMember]
        private double _profit;
        //[DataMember]
        private DateTime _timeOfCalculation;

        public TotalProductionHelper(double totalGeneration, double cO2Reduction, double cO2Emission, double totalCost, double profit, DateTime timeOfCalculation)
        {
            Id = 1;
            TotalGeneration = totalGeneration;
            CO2Reduction = cO2Reduction;
            CO2Emission = cO2Emission;
            TotalCost = totalCost;
            Profit = profit;
            TimeOfCalculation = timeOfCalculation;
        }


        public TotalProductionHelper()
        { }

        public TotalProductionHelper(TotalProduction totalProduction)
        {
            Id = 1;
            TotalGeneration = totalProduction.TotalGeneration;
            CO2Reduction = totalProduction.CO2Reduction;
            CO2Emission = totalProduction.CO2Emission;
            TotalCost = totalProduction.TotalCost;
            Profit = totalProduction.Profit;
            TimeOfCalculation = totalProduction.TimeOfCalculation;
        }

        [DataMember]
        public int Id { get => _id; set => _id = value; }
        [DataMember]
        public double TotalGeneration { get => _totalGeneration; set => _totalGeneration = value; }
        [DataMember]
        public double CO2Reduction { get => _cO2Reduction; set => _cO2Reduction = value; }
        [DataMember]
        public double CO2Emission { get => _cO2Emission; set => _cO2Emission = value; }
        [DataMember]
        public double TotalCost { get => _totalCost; set => _totalCost = value; }
        [DataMember]
        public double Profit { get => _profit; set => _profit = value; }
        [DataMember]
        public DateTime TimeOfCalculation { get => _timeOfCalculation; set => _timeOfCalculation = value; }


    }
}
