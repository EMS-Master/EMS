using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Meas
{
    public class Measurement : IdentifiedObject
    {
        private Direction direction;
        private MeasurementType measurmentType;
        private String scadaAddress;
        private long powerSystemResource = 0;

        public Measurement(long globalId) : base(globalId)
        {
        }

        public Direction Direction { get => direction; set => direction = value; }
        public MeasurementType MeasurmentType { get => measurmentType; set => measurmentType = value; }
        public string ScadaAddress { get => scadaAddress; set => scadaAddress = value; }
        public long PowerSystemResource { get => powerSystemResource; set => powerSystemResource = value; }


        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Measurement x = (Measurement)obj;
                return ((x.Direction == this.Direction && x.MeasurmentType == this.MeasurmentType && x.ScadaAddress== this.ScadaAddress));
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
                case ModelCode.MEASUREMENT_DIRECTION:
                case ModelCode.MEASUREMENT_SCADA_ADDRESS:
                case ModelCode.MEASUREMENT_TYPE:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.MEASUREMENT_DIRECTION:
                    prop.SetValue((short)direction);
                    break;
                case ModelCode.MEASUREMENT_SCADA_ADDRESS:
                    prop.SetValue(scadaAddress);
                    break;
                case ModelCode.MEASUREMENT_POWER_SYS_RESOURCE:
                    prop.SetValue(powerSystemResource);
                    break;
                case ModelCode.MEASUREMENT_TYPE:
                    prop.SetValue((short)measurmentType);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.MEASUREMENT_SCADA_ADDRESS:
                    scadaAddress = property.AsString();
                    break;
                case ModelCode.MEASUREMENT_DIRECTION:
                    direction = (Direction)property.AsEnum();
                    break;
                case ModelCode.MEASUREMENT_TYPE:
                    measurmentType = (MeasurementType)property.AsEnum();
                    break;
                case ModelCode.MEASUREMENT_POWER_SYS_RESOURCE:
                    powerSystemResource = property.AsReference();
                    break;

                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation	

        #region IReference implementation

        //public override bool IsReferenced
        //{
        //    get
        //    {
        //        return conductingEquipments.Count > 0 || base.IsReferenced;
        //    }
        //}


        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (powerSystemResource != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.MEASUREMENT_POWER_SYS_RESOURCE] = new List<long>();
                references[ModelCode.MEASUREMENT_POWER_SYS_RESOURCE].Add(powerSystemResource);
            }

            base.GetReferences(references, refType);
        }
        //public override void AddReference(ModelCode referenceId, long globalId)
        //{
        //    switch (referenceId)
        //    {
        //        case ModelCode.CONDEQ_BASVOLTAGE:
        //            conductingEquipments.Add(globalId);
        //            break;

        //        default:
        //            base.AddReference(referenceId, globalId);
        //            break;
        //    }
        //}

        //public override void RemoveReference(ModelCode referenceId, long globalId)
        //{
        //    switch (referenceId)
        //    {
        //        case ModelCode.CONDEQ_BASVOLTAGE:

        //            if (conductingEquipments.Contains(globalId))
        //            {
        //                conductingEquipments.Remove(globalId);
        //            }
        //            else
        //            {
        //                CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
        //            }

        //            break;

        //        default:
        //            base.RemoveReference(referenceId, globalId);
        //            break;
        //    }
        //}

        #endregion IReference implementation	
    }
}

