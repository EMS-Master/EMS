using CommonMeas;
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
    public class Alarm : TableEntity
    {
        //[DataMember]
        private int _id;
        //[DataMember]
        private long _gid;
        //[DataMember]
        private double _alarmValue;
        //[DataMember]
        private double _minValue;
        //[DataMember]
        private double _maxValue;
        //[DataMember]
        private DateTime _alarmTimeStamp;
        //[DataMember]
        private int _ackState;
        //[DataMember]
        private int _alarmType;
        //[DataMember]
        private string _alarmMessage;
        //[DataMember]
        private int _severity;
        //[DataMember]
        private string _currentState;
        //[DataMember]
        private int _pubStatus;
        //[DataMember]
        private string _name;

        [DataMember]
        public int Id { get => _id; set => _id = value; }
        [DataMember]
        public long Gid { get => _gid; set => _gid = value; }
        [DataMember]
        public double AlarmValue { get => _alarmValue; set => _alarmValue = value; }
        [DataMember]
        public double MinValue { get => _minValue; set => _minValue = value; }
        [DataMember]
        public double MaxValue { get => _maxValue; set => _maxValue = value; }
        [DataMember]
        public DateTime AlarmTimeStamp { get => _alarmTimeStamp; set => _alarmTimeStamp = value; }
        [DataMember]
        public int AckState { get => _ackState; set => _ackState = value; }
        [DataMember]
        public int AlarmType { get => _alarmType; set => _alarmType = value; }
        [DataMember]
        public string AlarmMessage { get => _alarmMessage; set => _alarmMessage = value; }
        [DataMember]
        public int Severity { get => _severity; set => _severity = value; }
        [DataMember]
        public string CurrentState { get => _currentState; set => _currentState = value; }
        [DataMember]
        public int PubStatus { get => _pubStatus; set => _pubStatus = value; }
        [DataMember]
        public string Name { get => _name; set => _name = value; }


        public Alarm(long gid, double value, double minValue, double maxValue, DateTime timeStamp)
        {
            this.Gid = gid;
            this.AlarmValue = value;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.AlarmTimeStamp = timeStamp;
            this.AlarmMessage = "";
            RowKey = gid.ToString() + "_" + DateTime.Now.ToString("o");
            PartitionKey = "Alarm";
        }

        public Alarm() { }
    }
}
