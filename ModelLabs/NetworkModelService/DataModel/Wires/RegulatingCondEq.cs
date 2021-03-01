using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    [DataContract]
    [Serializable()]
    public class RegulatingCondEq : ConductingEquipment
    {
        public RegulatingCondEq(long globalId) : base(globalId)
        {
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
                default:
                    return base.HasProperty(t);
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

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                
                default:
                    base.GetProperty(prop);
                    break;
            }
        }

        public override object Clone()
        {
            RegulatingCondEq io = new RegulatingCondEq(base.GlobalId);
            io.Measurements = this.Measurements;
            io.Mrid = this.Mrid;
            io.Name = this.Name;
            io.AliasName = this.AliasName;
            io.EquipmentContainer = this.EquipmentContainer;
            

            return io;
        }
        #endregion IAccess implementation

        #region IReference implementation

        //public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        //{
        //	if (baseVoltage != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
        //	{
        //		references[ModelCode.CONDEQ_BASVOLTAGE] = new List<long>();
        //		references[ModelCode.CONDEQ_BASVOLTAGE].Add(baseVoltage);
        //	}

        //	base.GetReferences(references, refType);
        //}

        #endregion IReference implementation
    }
}
