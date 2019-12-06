using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    public class RotatingMachine : RegulatingCondEq
    {
        private float ratedS;

        public RotatingMachine(long globalId) : base(globalId)
        {
        }

        /// <summary>
        /// Gets or sets RatedS of the entity
        /// </summary>
        public float RatedS
        {
            get { return this.ratedS; }
            set { this.ratedS = value; }
        }
        
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                RotatingMachine r = (RotatingMachine)obj;
                return r.ratedS == this.ratedS;
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
                case ModelCode.ROTATING_MACHINE_RATED_S:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }
        
        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.ROTATING_MACHINE_RATED_S:
                    prop.SetValue(this.ratedS);
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
                case ModelCode.ROTATING_MACHINE_RATED_S:
                    this.ratedS = property.AsFloat();
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
