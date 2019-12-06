using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Meas
{
    public class Discrete : Measurement
    {
        private int maxValue;
        private int minValue;
        private int normalValue;

        public Discrete(long globalId) : base(globalId)
        {
        }

        public int MaxValue { get => maxValue; set => maxValue = value; }
        public int MinValue { get => minValue; set => minValue = value; }
        public int NormalValue { get => normalValue; set => normalValue = value; }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Discrete r = (Discrete)obj;
                return (r.minValue == this.minValue && r.maxValue == this.maxValue && r.normalValue == this.normalValue);
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
                case ModelCode.DISCRETE_MAX_VALUE:
                    return true;
                case ModelCode.DISCRETE_MIN_VALUE:
                    return true;
                case ModelCode.DISCRETE_NORMAL_VALUE:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.DISCRETE_MAX_VALUE:
                    prop.SetValue(this.maxValue);
                    break;
                case ModelCode.DISCRETE_MIN_VALUE:
                    prop.SetValue(this.minValue);
                    break;
                case ModelCode.DISCRETE_NORMAL_VALUE:
                    prop.SetValue(this.normalValue);
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
                case ModelCode.DISCRETE_MAX_VALUE:
                    this.maxValue = property.AsInt();
                    break;
                case ModelCode.DISCRETE_MIN_VALUE:
                    this.minValue = property.AsInt();
                    break;
                case ModelCode.DISCRETE_NORMAL_VALUE:
                    this.normalValue = property.AsInt();
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
