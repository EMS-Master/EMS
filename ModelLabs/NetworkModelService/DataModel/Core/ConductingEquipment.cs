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
	public class ConductingEquipment : Equipment
	{		
		
			
		public ConductingEquipment(long globalId) : base(globalId) 
		{
		}

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
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
