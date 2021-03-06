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
    
    
    /// A winding is associated with each defined terminal of a transformer (or phase shifter).
    public class TransformerWinding : ConductingEquipment {
        
        /// The type of connection of the winding.
        private WindingConnection? cim_connectionType;
        
        private const bool isConnectionTypeMandatory = true;
        
        private const string _connectionTypePrefix = "cim";
        
        /// Set if the winding is grounded.
        private System.Boolean? cim_grounded;
        
        private const bool isGroundedMandatory = true;
        
        private const string _groundedPrefix = "cim";
        
        /// Phase-to-ground voltage of the winding.
        private System.Single? cim_phaseToGroundVoltage;
        
        private const bool isPhaseToGroundVoltageMandatory = false;
        
        private const string _phaseToGroundVoltagePrefix = "ftn";
        
        /// Phase-to-phase voltage of the winding.
        private System.Single? cim_phaseToPhaseVoltage;
        
        private const bool isPhaseToPhaseVoltageMandatory = false;
        
        private const string _phaseToPhaseVoltagePrefix = "ftn";
        
        /// A transformer has windings
        private PowerTransformer cim_PowerTransformer;
        
        private const bool isPowerTransformerMandatory = true;
        
        private const string _PowerTransformerPrefix = "cim";
        
        /// The normal apparent power rating for the winding
        private System.Single? cim_ratedS;
        
        private const bool isRatedSMandatory = true;
        
        private const string _ratedSPrefix = "cim";
        
        /// The rated voltage (phase-to-phase) of the winding, usually the same as the neutral voltage.
        private System.Single? cim_ratedU;
        
        private const bool isRatedUMandatory = false;
        
        private const string _ratedUPrefix = "cim";
        
        /// The type of winding.
        private WindingType? cim_windingType;
        
        private const bool isWindingTypeMandatory = true;
        
        private const string _windingTypePrefix = "cim";
        
        public virtual WindingConnection ConnectionType {
            get {
                return this.cim_connectionType.GetValueOrDefault();
            }
            set {
                this.cim_connectionType = value;
            }
        }
        
        public virtual bool ConnectionTypeHasValue {
            get {
                return this.cim_connectionType != null;
            }
        }
        
        public static bool IsConnectionTypeMandatory {
            get {
                return isConnectionTypeMandatory;
            }
        }
        
        public static string ConnectionTypePrefix {
            get {
                return _connectionTypePrefix;
            }
        }
        
        public virtual bool Grounded {
            get {
                return this.cim_grounded.GetValueOrDefault();
            }
            set {
                this.cim_grounded = value;
            }
        }
        
        public virtual bool GroundedHasValue {
            get {
                return this.cim_grounded != null;
            }
        }
        
        public static bool IsGroundedMandatory {
            get {
                return isGroundedMandatory;
            }
        }
        
        public static string GroundedPrefix {
            get {
                return _groundedPrefix;
            }
        }
        
        public virtual float PhaseToGroundVoltage {
            get {
                return this.cim_phaseToGroundVoltage.GetValueOrDefault();
            }
            set {
                this.cim_phaseToGroundVoltage = value;
            }
        }
        
        public virtual bool PhaseToGroundVoltageHasValue {
            get {
                return this.cim_phaseToGroundVoltage != null;
            }
        }
        
        public static bool IsPhaseToGroundVoltageMandatory {
            get {
                return isPhaseToGroundVoltageMandatory;
            }
        }
        
        public static string PhaseToGroundVoltagePrefix {
            get {
                return _phaseToGroundVoltagePrefix;
            }
        }
        
        public virtual float PhaseToPhaseVoltage {
            get {
                return this.cim_phaseToPhaseVoltage.GetValueOrDefault();
            }
            set {
                this.cim_phaseToPhaseVoltage = value;
            }
        }
        
        public virtual bool PhaseToPhaseVoltageHasValue {
            get {
                return this.cim_phaseToPhaseVoltage != null;
            }
        }
        
        public static bool IsPhaseToPhaseVoltageMandatory {
            get {
                return isPhaseToPhaseVoltageMandatory;
            }
        }
        
        public static string PhaseToPhaseVoltagePrefix {
            get {
                return _phaseToPhaseVoltagePrefix;
            }
        }
        
        public virtual PowerTransformer PowerTransformer {
            get {
                return this.cim_PowerTransformer;
            }
            set {
                this.cim_PowerTransformer = value;
            }
        }
        
        public virtual bool PowerTransformerHasValue {
            get {
                return this.cim_PowerTransformer != null;
            }
        }
        
        public static bool IsPowerTransformerMandatory {
            get {
                return isPowerTransformerMandatory;
            }
        }
        
        public static string PowerTransformerPrefix {
            get {
                return _PowerTransformerPrefix;
            }
        }
        
        public virtual float RatedS {
            get {
                return this.cim_ratedS.GetValueOrDefault();
            }
            set {
                this.cim_ratedS = value;
            }
        }
        
        public virtual bool RatedSHasValue {
            get {
                return this.cim_ratedS != null;
            }
        }
        
        public static bool IsRatedSMandatory {
            get {
                return isRatedSMandatory;
            }
        }
        
        public static string RatedSPrefix {
            get {
                return _ratedSPrefix;
            }
        }
        
        public virtual float RatedU {
            get {
                return this.cim_ratedU.GetValueOrDefault();
            }
            set {
                this.cim_ratedU = value;
            }
        }
        
        public virtual bool RatedUHasValue {
            get {
                return this.cim_ratedU != null;
            }
        }
        
        public static bool IsRatedUMandatory {
            get {
                return isRatedUMandatory;
            }
        }
        
        public static string RatedUPrefix {
            get {
                return _ratedUPrefix;
            }
        }
        
        public virtual WindingType WindingType {
            get {
                return this.cim_windingType.GetValueOrDefault();
            }
            set {
                this.cim_windingType = value;
            }
        }
        
        public virtual bool WindingTypeHasValue {
            get {
                return this.cim_windingType != null;
            }
        }
        
        public static bool IsWindingTypeMandatory {
            get {
                return isWindingTypeMandatory;
            }
        }
        
        public static string WindingTypePrefix {
            get {
                return _windingTypePrefix;
            }
        }
    }
}
