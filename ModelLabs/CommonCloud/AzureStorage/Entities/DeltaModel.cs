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
    public class DeltaModel : TableEntity
    {
        [DataMember]
        private int id;
        [DataMember]
        private DateTime time;
        [DataMember]
        private byte[] delta;

        public int Id { get => id; set => id = value; }
        public DateTime Time { get => time; set => time = value; }
        public byte[] Delta { get => delta; set => delta = value; }

        public DeltaModel()
        { }

        public DeltaModel(int id, DateTime time, byte[] delta)
        {
            Id = id;
            Time = time;
            Delta = delta;
            RowKey = DateTime.Now.ToString("o");
            PartitionKey = "DeltaModel";
        }
    }
}
