using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Meas
{
    public class Analog : Measurement
    {
        private float maxValue;
        private float minValue;
        private float normalValue;

        public Analog(long globalId) : base(globalId)
        {
        }

        public Analog() 
        {
        }

        public float MaxValue { get => maxValue; set => maxValue = value; }
        public float MinValue { get => minValue; set => minValue = value; }
        public float NormalValue { get => normalValue; set => normalValue = value; }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Analog r = (Analog)obj;
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

        public override object Clone()
        {
            Analog io = new Analog();
            io.MaxValue = this.MaxValue;
            io.MeasurmentType = this.MeasurmentType;
            io.MinValue = this.MinValue;
            io.Mrid = this.Mrid;
            io.Name = this.Name;
            io.AliasName = this.AliasName;
            io.NormalValue = this.NormalValue;
            io.PowerSystemResource = this.PowerSystemResource;
            io.Direction = this.Direction;
            io.ScadaAddress = this.ScadaAddress;

            return io;
        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode t)
        {
            switch (t)
            {
                case ModelCode.ANALOG_MAX_VALUE:
                    return true;
                case ModelCode.ANALOG_MIN_VALUE:
                    return true;
                case ModelCode.ANALOG_NORMAL_VALUE:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.ANALOG_MAX_VALUE:
                    prop.SetValue(this.maxValue);
                    break;
                case ModelCode.ANALOG_MIN_VALUE:
                    prop.SetValue(this.minValue);
                    break;
                case ModelCode.ANALOG_NORMAL_VALUE:
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
                case ModelCode.ANALOG_MAX_VALUE:
                    this.maxValue = property.AsFloat();
                    break;
                case ModelCode.ANALOG_MIN_VALUE:
                    this.minValue = property.AsFloat();
                    break;
                case ModelCode.ANALOG_NORMAL_VALUE:
                    this.normalValue = property.AsFloat();
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
