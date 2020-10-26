using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMeas
{
    public enum AlarmType : int
    {
        NONE = 0,
        HIGH = 1,
        LOW = 2,
        DOM = 3,
        ABNORMAL = 4,
        NORMAL = 5
    }

    public enum SeverityLevel : int
    {
        NORMAL = 0,
        MINOR = 1,       // cyan boje su izmenjene...
        LOW = 2,           // green 
        MEDIUM = 3,          // yellow
        MAJOR = 4,         // orange
        HIGH = 5,            // red
        CRITICAL = 6           // magenta
    }

    public enum AckState : int
    {
        Acknowledged = 0,
        Unacknowledged = 1
    }

    public enum PublishingStatus : int
    {
        INSERT = 0,
        UPDATE = 1
    }

   
    public enum State : int
    {
        Active = 0,
        Cleared = 1,
    }

    public enum PersistentState : int
    {
        Persistent = 0,
        Nonpersistent = 1,
    }


}
