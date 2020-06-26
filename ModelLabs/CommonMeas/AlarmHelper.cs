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

        private AckState ackState;
   
        private AlarmType type;

        private string currentState;
      
        private PublishingStatus pubStatus;

        // type of alarm - persistent or not
        private PersistentState persistent;

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

        public AlarmHelper(long gid, float value)
        {
            this.gid = gid;
            this.value = value;         
            this.message = "";
            this.persistent = PersistentState.Persistent;


        }

        public long Gid { get { return this.gid; } set { this.gid = value; } }

       
        public AckState AckState { get { return ackState; } set { ackState = value; } }



        public PersistentState Persistent
        {
            get
            {
                return persistent;
            }
            set
            {
                persistent = value;
            }
        }


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

       


    }
}
