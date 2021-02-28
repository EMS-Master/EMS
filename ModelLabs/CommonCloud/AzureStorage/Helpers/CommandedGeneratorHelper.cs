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
    public class CommandedGeneratorHelper
    {
        //[DataMember]
        private int _id;
        //[DataMember]
        private long _gid;
        //[DataMember]
        private bool _commandingFlag;
        //[DataMember]
        private double _commandingValue;

        [DataMember]
        public int Id { get => _id; set => _id = value; }
        [DataMember]
        public long Gid { get => _gid; set => _gid = value; }
        [DataMember]
        public bool CommandingFlag { get => _commandingFlag; set => _commandingFlag = value; }
        [DataMember]
        public double CommandingValue { get => _commandingValue; set => _commandingValue = value; }

        public CommandedGeneratorHelper()
        { }

        public CommandedGeneratorHelper(int id, long gid, bool commandingFlag, double commandingValue)
        {
            Id = id;
            Gid = gid;
            CommandingFlag = commandingFlag;
            CommandingValue = commandingValue;
        }

        public CommandedGeneratorHelper(CommandedGenerator commandedGenerator)
        {
            Id = commandedGenerator.Id;
            Gid = commandedGenerator.Gid;
            CommandingFlag = commandedGenerator.CommandingFlag;
            CommandingValue = commandedGenerator.CommandingValue;
        }
    }
}
