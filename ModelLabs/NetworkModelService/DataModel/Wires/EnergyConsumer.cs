using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    public class EnergyConsumer : ConductingEquipment
    {
        private float currentPower;
        private float pFixed;
        public EnergyConsumer(long globalId) : base(globalId)
        {
        }

        public float CurrentPower { get => currentPower; set => currentPower = value; }
        public float PFixed { get => pFixed; set => pFixed = value; }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                EnergyConsumer r = (EnergyConsumer)obj;
                return (r.currentPower == this.currentPower && r.pFixed == this.pFixed);
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
            EnergyConsumer io = new EnergyConsumer(base.GlobalId);
            io.Measurements = this.Measurements;
            io.Mrid = this.Mrid;
            io.Name = this.Name;
            io.AliasName = this.AliasName;
            io.PFixed = this.PFixed;
            io.CurrentPower = this.CurrentPower;
            io.EquipmentContainer = this.EquipmentContainer;

            return io;

        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode t)
        {
            switch (t)
            {
                case ModelCode.ENERGY_CONSUMER_CURRENT_POWER:
                    return true;
                case ModelCode.ENERGY_CONSUMER_PFIXED:
                    return true;
                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.ENERGY_CONSUMER_CURRENT_POWER:
                    prop.SetValue(this.currentPower);
                    break;
                case ModelCode.ENERGY_CONSUMER_PFIXED:
                    prop.SetValue(this.pFixed);
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
                case ModelCode.ENERGY_CONSUMER_CURRENT_POWER:
                    this.currentPower = property.AsFloat();
                    break;
                case ModelCode.ENERGY_CONSUMER_PFIXED:
                    this.pFixed = property.AsFloat();
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
