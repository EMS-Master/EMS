using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using FTN.Common;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
	public class Equipment : PowerSystemResource
	{
        private long equipmentContainer = 0;

        public Equipment(long globalId) : base(globalId) 
		{
		}

        public long EquipmentContainer { get => equipmentContainer; set => equipmentContainer = value; }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Equipment x = (Equipment)obj;
                return ((x.EquipmentContainer == this.EquipmentContainer));
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
                case ModelCode.EQUIPMENT_EQUIPMENT_CONTAINER:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override object Clone()
        {
            Equipment io = new Equipment(base.GlobalId);
            io.Measurements = this.Measurements;
            io.AliasName = this.AliasName;
            io.EquipmentContainer = this.EquipmentContainer;
            io.Mrid = this.Mrid;
            io.Name = this.Name;
            return io;
        }
        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {                
                case ModelCode.EQUIPMENT_EQUIPMENT_CONTAINER:
                    prop.SetValue(equipmentContainer);
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
                
                case ModelCode.EQUIPMENT_EQUIPMENT_CONTAINER:
                    equipmentContainer = property.AsReference();
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
            if (equipmentContainer != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.EQUIPMENT_EQUIPMENT_CONTAINER] = new List<long>();
                references[ModelCode.EQUIPMENT_EQUIPMENT_CONTAINER].Add(equipmentContainer);
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
