using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonMeas
{
    public class AlarmHelper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // stores gid of the entity
        private long gid;

        //represents alarm severity level-coloring
        private SeverityLevel severity;

        //stores value of the entity
        private float value;

        //value that triggers the alarm
        private float initiatingValue;

        //stores min value of the entity
        private float minValue;

        //stores max value of the entity
        private float maxValue;

        //stores time stamp of the entity
        private DateTime timeStamp;

        //stores last change of the entity
        private DateTime lastChange;

        private string currentState;

        private AckState ackState;

        private PublishingStatus pubStatus;

        private AlarmType type;

        //type of alarm: persistent or not
        private PersistentState persistent;

        //type of alarm: inhibit or not
        private InhibitState inhibit;

        //stores the message
        private string message;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

     
        public AlarmHelper() { }

        public AlarmHelper(long gid, float value, float minValue, float maxValue, DateTime timeStamp)
        {
            this.gid = gid;
            this.value = value;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.timeStamp = timeStamp;
            this.message = "";
            this.persistent = PersistentState.Persistent;
            this.inhibit = InhibitState.Noninhibit;
            this.lastChange = timeStamp;
        }

        public long Gid { get { return this.gid; } set { this.gid = value; } }

        public float MinValue { get { return this.minValue; } set { this.minValue = value; } }

        public float MaxValue { get { return this.maxValue; } set { this.maxValue = value; } }

        public AckState AckState { get { return ackState; } set { ackState = value; } }

        public PersistentState Persisent { get { return persistent; } set { persistent = value; } }

        public InhibitState Inhibit { get { return inhibit; } set { inhibit = value; } }


        public SeverityLevel Severity
        {
            get
            {
                return severity;
            }
            set
            {
                severity = value;
                NotifyPropertyChanged();
            }
        }

        public float Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
                NotifyPropertyChanged();
            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return this.timeStamp;
            }

            set
            {
                this.timeStamp = value;
                NotifyPropertyChanged();
            }
        }

        public DateTime LastChange
        {
            get
            {
                return this.lastChange;
            }

            set
            {
                this.lastChange = value;
                NotifyPropertyChanged();
            }
        }

        public AlarmType Type
        {
            get
            {
                return this.type;
            }

            set
            {
                this.type = value;
                NotifyPropertyChanged();
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
                NotifyPropertyChanged();
            }
        }

        public string CurrentState
        {
            get
            {
                return this.currentState;
            }
            set
            {
                this.currentState = value;
                NotifyPropertyChanged();
            }
        }

        public PublishingStatus PubStatus
        {
            get
            {
                return pubStatus;
            }
            set
            {
                pubStatus = value;
                NotifyPropertyChanged();
            }
        }

        public float InitiatingValue
        {
            get
            {
                return initiatingValue;
            }
            set
            {
                initiatingValue = value;
                NotifyPropertyChanged();
            }
        }


    }
}
