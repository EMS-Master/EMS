using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    [DataContract]
    [Serializable()]
    public class Generator : RotatingMachine
    {
        private float minQ;
        private float maxQ;
        private GeneratorType generatorType;
        private float x;
        private float y;

        [DataMember]
        public float MinQ { get => minQ; set => minQ = value; }
        [DataMember]
        public float MaxQ { get => maxQ; set => maxQ = value; }
        [DataMember]
        public GeneratorType GeneratorType { get => generatorType; set => generatorType = value; }
        [DataMember]
        public float X { get => x; set => x = value; }
        [DataMember]
        public float Y { get => y; set => y = value; }

        public Generator(long globalId) : base(globalId)
        {
        }

        public Generator(long globalId, float minQ, float maxQ, GeneratorType generatorType, float x, float y) : base(globalId)
        {
            this.MinQ = minQ;
            this.MaxQ = maxQ;
            this.GeneratorType = generatorType;
            this.GlobalId = globalId;
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Generator r = (Generator)obj;
                return (r.MinQ == this.MinQ && r.MaxQ == this.MaxQ && r.generatorType == this.generatorType);
            }
            else
            {
                return false;
            }
        }

        public override object Clone()
        {
            Generator io = new Generator(base.GlobalId);
            io.AliasName = this.AliasName;
            io.EquipmentContainer = this.EquipmentContainer;
            io.GeneratorType = this.GeneratorType;
            io.Mrid = this.Mrid;
            io.MaxQ = this.MaxQ;
            io.Measurements = this.Measurements;
            io.X = this.X;
            io.MinQ = this.MinQ;
            io.Mrid = this.Mrid;
            io.Name = this.Name;
            io.Y = this.Y;
            io.RatedS = this.RatedS;

            return io;

        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        #region IAccess implementation

        public override bool HasProperty(ModelCode t)
        {
            switch (t)
            {
                case ModelCode.GENERATOR_MAX_Q:
                    return true;
                case ModelCode.GENERATOR_MIN_Q:
                    return true;
                case ModelCode.GENERATOR_TYPE:
                    return true;
                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.GENERATOR_MAX_Q:
                    prop.SetValue(this.MaxQ);
                    break;
                case ModelCode.GENERATOR_MIN_Q:
                    prop.SetValue(this.MinQ);
                    break;
                case ModelCode.GENERATOR_TYPE:
                    prop.SetValue((short)this.GeneratorType);
                    break;
                default:
                    base.GetProperty(prop);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.GENERATOR_MAX_Q:
                    this.MaxQ = property.AsFloat();
                    break;
                case ModelCode.GENERATOR_MIN_Q:
                    this.MinQ = property.AsFloat();
                    break;

                case ModelCode.GENERATOR_TYPE:
                    this.GeneratorType = (GeneratorType)property.AsEnum();
                    break;
                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        #region IReference implementation

        #endregion IReference implementation
    }
}
