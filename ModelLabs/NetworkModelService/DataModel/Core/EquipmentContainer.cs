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
            if (base.Equals(obj))
            {
                EquipmentContainer x = (EquipmentContainer)obj;
                return ((CompareHelper.CompareLists(x.equpments, this.equpments)));
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
        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (equpments != null && equpments.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.EQUIPMENT_EQ_CONTAINER] = equpments.GetRange(0, equpments.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.EQUIPMENT_EQ_CONTAINER:
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
                case ModelCode.EQUIPMENT_EQ_CONTAINER:

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
        //public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        //{
        //	if (location != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
        //	{
        //		references[ModelCode.PSR_LOCATION] = new List<long>();
        //		references[ModelCode.PSR_LOCATION].Add(location);
        //	}

        //	base.GetReferences(references, refType);			
        //}

        #endregion IReference implementation		
    }
}
