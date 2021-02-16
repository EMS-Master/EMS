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
    public class HistoryMeasurement : TableEntity
    {
        [DataMember]
        private int _id;
        [DataMember]
        private long _gid;
        [DataMember]
        private DateTime _measurementTime;
        [DataMember]
        private double _measurementValue;

        public HistoryMeasurement( long gid, DateTime measurementTime, double measurementValue)
        {
            Id = 1;
            Gid = gid;
            MeasurementTime = measurementTime;
            MeasurementValue = measurementValue;
            RowKey = gid.ToString()+ "_" + DateTime.Now.ToString("o");
            PartitionKey = "HistoryMeasurement";
        }

        public HistoryMeasurement()
        { }

        public int Id { get => _id; set => _id = value; }
        public long Gid { get => _gid; set => _gid = value; }
        public DateTime MeasurementTime { get => _measurementTime; set => _measurementTime = value; }
        public double MeasurementValue { get => _measurementValue; set => _measurementValue = value; }
    }
}
