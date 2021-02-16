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
    public class CommandedGenerator : TableEntity
    {
        [DataMember]
        private int _id;
        [DataMember]
        private long _gid;
        [DataMember]
        private bool _commandingFlag;
        [DataMember]
        private double _commandingValue;

        public int Id { get => _id; set => _id = value; }
        public long Gid { get => _gid; set => _gid = value; }
        public bool CommandingFlag { get => _commandingFlag; set => _commandingFlag = value; }
        public double CommandingValue { get => _commandingValue; set => _commandingValue = value; }

        public CommandedGenerator()
            {}

        public CommandedGenerator(int id, long gid, bool commandingFlag, double commandingValue)
        {
            Id = id;
            Gid = gid;
            CommandingFlag = commandingFlag;
            CommandingValue = commandingValue;
            RowKey = gid.ToString() +"_" + DateTime.Now.ToString("o");
            PartitionKey = "CommandedGenerator";
        }
    }
}
