using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    public class EquipmentContainer : ConnectivityNodeContainer
    {
        private List<long> equpments = new List<long>();

       

        public EquipmentContainer(long globalId)
            : base(globalId)
        {
        }


        public List<long> Equpments { get => equpments; set => equpments = value; }

        public override bool Equals(object obj)
        {

            EquipmentContainer x = (EquipmentContainer)obj;
            if (base.Equals(obj))
            {
                return ((CompareHelper.CompareLists(x.equpments, this.equpments)));
            }
            else
            {
                return false;
            }
        }
        public override object Clone()
        {
            EquipmentContainer io = new EquipmentContainer(base.GlobalId);
            io.Measurements = this.Measurements;
            io.AliasName = this.AliasName;
            io.Mrid = this.Mrid;
            io.Name = this.Name;
            io.Equpments = this.Equpments;
            

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
                case ModelCode.EQUIPMENT_CONTAINER_EQUIPMENTS:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.EQUIPMENT_CONTAINER_EQUIPMENTS:
                    prop.SetValue(equpments);
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
                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        #region IReference implementation

        public override bool IsReferenced
        {
            get
            {
                return (equpments.Count > 0) || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (equpments != null && equpments.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.EQUIPMENT_CONTAINER_EQUIPMENTS] = equpments.GetRange(0, equpments.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.EQUIPMENT_EQUIPMENT_CONTAINER:
                    equpments.Add(globalId);
                    break;

                default:
                    base.AddReference(referenceId, globalId);
                    break;
            }
        }

        public override void RemoveReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.EQUIPMENT_EQUIPMENT_CONTAINER:

                    if (equpments.Contains(globalId))
                    {
                        equpments.Remove(globalId);
                    }
                    else
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
                    }

                    break;

                default:
                    base.RemoveReference(referenceId, globalId);
                    break;
            }
        }
        

        #endregion IReference implementation		
    }
}
