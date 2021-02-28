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
    public class HistoryMeasurementHelper 
    {
        //[DataMember]
        private int _id;
        //[DataMember]
        private long _gid;
        //[DataMember]
        private DateTime _measurementTime;
        //[DataMember]
        private double _measurementValue;

        public HistoryMeasurementHelper(
            long gid, 
            DateTime measurementTime, 
            double measurementValue)
        {
            Id = 1;
            Gid = gid;
            MeasurementTime = measurementTime;
            MeasurementValue = measurementValue;
        }

        public HistoryMeasurementHelper()
        { }

        [DataMember]
        public int Id { get => _id; set => _id = value; }
        [DataMember]
        public long Gid { get => _gid; set => _gid = value; }
        [DataMember]
        public DateTime MeasurementTime { get => _measurementTime; set => _measurementTime = value; }
        [DataMember]
        public double MeasurementValue { get => _measurementValue; set => _measurementValue = value; }
    }
}
