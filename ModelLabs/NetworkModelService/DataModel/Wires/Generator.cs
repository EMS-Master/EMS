using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    public class Generator : RotatingMachine
    {
        private float minQ;
        private float maxQ;
        private GeneratorType generatorType;

        public float MinQ { get => minQ; set => minQ = value; }
        public float MaxQ { get => maxQ; set => maxQ = value; }
        public GeneratorType GeneratorType { get => generatorType; set => generatorType = value; }

        public Generator(long globalId) : base(globalId)
        {
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
                    prop.SetValue(this.maxQ);
                    break;
                case ModelCode.GENERATOR_MIN_Q:
                    prop.SetValue(this.minQ);
                    break;
                case ModelCode.GENERATOR_TYPE:
                    prop.SetValue((short)this.generatorType);
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
                    this.maxQ = property.AsFloat();
                    break;
                case ModelCode.GENERATOR_MIN_Q:
                    this.minQ = property.AsFloat();
                    break;

                case ModelCode.GENERATOR_TYPE:
                    this.generatorType = (GeneratorType)property.AsEnum();
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
