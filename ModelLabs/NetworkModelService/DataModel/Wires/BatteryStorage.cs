using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    //public class BatteryStorage : EnergyConsumer
    //{
    //    private float maxPower;
    //    private float minCapacity;
    //    private float x;
    //    private float y;

    //    public BatteryStorage(long globalId,float maxPower, float minCapacity, float x, float y) : base(globalId)
    //    {
    //        this.MaxPower = maxPower;
    //        this.MinCapacity = minCapacity;
    //        this.X = x;
    //        this.Y = y;
    //    }

    //    public BatteryStorage(long globalId) : base(globalId)
    //    {
    //    }

    //    public float MaxPower { get => maxPower; set => maxPower = value; }
    //    public float MinCapacity { get => minCapacity; set => minCapacity = value; }
    //    public float X { get => x; set => x = value; }
    //    public float Y { get => y; set => y = value; }

    //    public override bool Equals(object obj)
    //    {
    //        if (base.Equals(obj))
    //        {
    //            BatteryStorage r = (BatteryStorage)obj;
    //            return (r.maxPower == this.maxPower && r.minCapacity == this.minCapacity);
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    public override int GetHashCode()
    //    {
    //        return base.GetHashCode();
    //    }


    //    #region IAccess implementation

    //    public override bool HasProperty(ModelCode t)
    //    {
    //        switch (t)
    //        {
    //            case ModelCode.BATTERY_STORAGE_MAX_POWER:
    //                return true;
    //            case ModelCode.BATTERY_STORAGE_MIN_CAPACITY:
    //                return true;
    //            default:
    //                return base.HasProperty(t);
    //        }
    //    }

    //    public override void GetProperty(Property prop)
    //    {
    //        switch (prop.Id)
    //        {
    //            case ModelCode.BATTERY_STORAGE_MAX_POWER:
    //                prop.SetValue(this.maxPower);
    //                break;
    //            case ModelCode.BATTERY_STORAGE_MIN_CAPACITY:
    //                prop.SetValue(this.minCapacity);
    //                break;
    //            default:
    //                base.GetProperty(prop);
    //                break;
    //        }
    //    }

    //    public override void SetProperty(Property property)
    //    {
    //        switch (property.Id)
    //        {
    //            case ModelCode.BATTERY_STORAGE_MAX_POWER:
    //                this.maxPower = property.AsFloat();
    //                break;
    //            case ModelCode.BATTERY_STORAGE_MIN_CAPACITY:
    //                this.minCapacity = property.AsFloat();
    //                break;
    //            default:
    //                base.SetProperty(property);
    //                break;
    //        }
    //    }

    //    #endregion IAccess implementation

    //    #region IReference implementation

    //    #endregion IReference implementation
    //}
}
