using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace CommonCloud.AzureStorage.Entities
{
    [DataContract]
    [Serializable()]
    public class DiscreteCounter : TableEntity
    {
        [DataMember]
        private int id;
        [DataMember]
        private long gid;
        [DataMember]
        private bool currentValue;
        [DataMember]
        private int counter;
        [DataMember]
        private string name;

        public int Id { get => id; set => id = value; }
        public long Gid { get => gid; set => gid = value; }
        public bool CurrentValue { get => currentValue; set => currentValue = value; }
        public int Counter { get => counter; set => counter = value; }
        public string Name { get => name; set => name = value; }

        public DiscreteCounter(int id, long gid, bool currentValue, int counter, string name)
        {
            Id = id;
            Gid = gid;
            CurrentValue = currentValue;
            Counter = counter;
            Name = name;
            RowKey = Id.ToString() ;
            PartitionKey = "DiscreteCounter";
            Timestamp = DateTime.Now;
        }

        public DiscreteCounter()
        { }
    }
}
