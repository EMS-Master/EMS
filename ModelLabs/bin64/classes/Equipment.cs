//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FTN {
    using System;
    using FTN;
    
    
    /// The parts of a power system that are physical devices, electronic or mechanical
    public class Equipment : PowerSystemResource {
        
        /// True if device belongs to underground layer.
        private System.Boolean? cim_isUnderground;
        
        private const bool isIsUndergroundMandatory = false;
        
        private const string _isUndergroundPrefix = "ftn";
        
        /// True if equipment is private property.
        private System.Boolean? cim_private;
        
        private const bool isPrivateMandatory = false;
        
        private const string _privatePrefix = "ftn";
        
        public virtual bool IsUnderground {
            get {
                return this.cim_isUnderground.GetValueOrDefault();
            }
            set {
                this.cim_isUnderground = value;
            }
        }
        
        public virtual bool IsUndergroundHasValue {
            get {
                return this.cim_isUnderground != null;
            }
        }
        
        public static bool IsIsUndergroundMandatory {
            get {
                return isIsUndergroundMandatory;
            }
        }
        
        public static string IsUndergroundPrefix {
            get {
                return _isUndergroundPrefix;
            }
        }
        
        public virtual bool Private {
            get {
                return this.cim_private.GetValueOrDefault();
            }
            set {
                this.cim_private = value;
            }
        }
        
        public virtual bool PrivateHasValue {
            get {
                return this.cim_private != null;
            }
        }
        
        public static bool IsPrivateMandatory {
            get {
                return isPrivateMandatory;
            }
        }
        
        public static string PrivatePrefix {
            get {
                return _privatePrefix;
            }
        }
    }
}
