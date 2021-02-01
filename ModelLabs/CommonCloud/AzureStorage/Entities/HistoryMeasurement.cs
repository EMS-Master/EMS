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
        private float _measurementValue;

        public HistoryMeasurement(int id, long gid, DateTime measurementTime, float measurementValue)
        {
            Id = id;
            Gid = gid;
            MeasurementTime = measurementTime;
            MeasurementValue = measurementValue;
            RowKey = Id.ToString();
            PartitionKey = "HistoryMeasurement";
            Timestamp = DateTime.Now;
        }

        public HistoryMeasurement()
        { }

        public int Id { get => _id; set => _id = value; }
        public long Gid { get => _gid; set => _gid = value; }
        public DateTime MeasurementTime { get => _measurementTime; set => _measurementTime = value; }
        public float MeasurementValue { get => _measurementValue; set => _measurementValue = value; }
    }
}
