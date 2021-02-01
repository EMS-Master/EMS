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
        [DataMember]
        private int _id;
        [DataMember]
        private long _gid;
        [DataMember]
        private float _alarmValue;
        [DataMember]
        private float _minValue;
        [DataMember]
        private float _maxValue;
        [DataMember]
        private DateTime _alarmTimeStamp;
        [DataMember]
        private AckState _ackState;
        [DataMember]
        private AlarmType _alarmType;
        [DataMember]
        private string _alarmMessage;
        [DataMember]
        private SeverityLevel _severity;
        [DataMember]
        private string _currentState;
        [DataMember]
        private PublishingStatus _pubStatus;
        [DataMember]
        private string _name;

        public int Id { get => _id; set => _id = value; }
        public long Gid { get => _gid; set => _gid = value; }
        public float AlarmValue { get => _alarmValue; set => _alarmValue = value; }
        public float MinValue { get => _minValue; set => _minValue = value; }
        public float MaxValue { get => _maxValue; set => _maxValue = value; }
        public DateTime AlarmTimeStamp { get => _alarmTimeStamp; set => _alarmTimeStamp = value; }
        public AckState AckState { get => _ackState; set => _ackState = value; }
        public AlarmType AlarmType { get => _alarmType; set => _alarmType = value; }
        public string AlarmMessage { get => _alarmMessage; set => _alarmMessage = value; }
        public SeverityLevel Severity { get => _severity; set => _severity = value; }
        public string CurrentState { get => _currentState; set => _currentState = value; }
        public PublishingStatus PubStatus { get => _pubStatus; set => _pubStatus = value; }
        public string Name { get => _name; set => _name = value; }


        public Alarm(long gid, float value, float minValue, float maxValue, DateTime timeStamp)
        {
            this.Gid = gid;
            this.AlarmValue = value;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.AlarmTimeStamp = timeStamp;
            this.AlarmMessage = "";
            RowKey = Id.ToString();
            PartitionKey = "Alarm";
            Timestamp = DateTime.Now;
        }

        public Alarm() { }
    }
}
