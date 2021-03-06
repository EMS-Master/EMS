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
        //[DataMember]
        private int id;
        //[DataMember]
        private long gid;
        //[DataMember]
        private bool currentValue;
        //[DataMember]
        private int counter;
        //[DataMember]
        private string name;

        [DataMember]
        public int Id { get => id; set => id = value; }
        [DataMember]
        public long Gid { get => gid; set => gid = value; }
        [DataMember]
        public bool CurrentValue { get => currentValue; set => currentValue = value; }
        [DataMember]
        public int Counter { get => counter; set => counter = value; }
        [DataMember]
        public string Name { get => name; set => name = value; }

        public DiscreteCounter( long gid, bool currentValue, int counter, string name)
        {
            Id = 1;
            Gid = gid;
            CurrentValue = currentValue;
            Counter = counter;
            Name = name;
            RowKey = gid.ToString();// + "_" + DateTime.Now.ToString("o");
            PartitionKey = "DiscreteCounter";
        }

        public DiscreteCounter()
        { }
    }
}
