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
    
    
    /// An electrical device consisting of  two or more coupled windings, with or without a magnetic core, for introducing mutual coupling between electric circuits. Transformers can be used to control voltage and phase shift (active power flow).
    public class PowerTransformer : Equipment {
        
        /// True if the transformer is an autotransfomer
        private System.Boolean? cim_autotransformer;
        
        private const bool isAutotransformerMandatory = false;
        
        private const string _autotransformerPrefix = "ftn";
        
        /// Purpose of the transformer within the network. (Copied from 61970:TransformerAsset)
        private TransformerFunctionKind? cim_function;
        
        private const bool isFunctionMandatory = true;
        
        private const string _functionPrefix = "ftn";
        
        public virtual bool Autotransformer {
            get {
                return this.cim_autotransformer.GetValueOrDefault();
            }
            set {
                this.cim_autotransformer = value;
            }
        }
        
        public virtual bool AutotransformerHasValue {
            get {
                return this.cim_autotransformer != null;
            }
        }
        
        public static bool IsAutotransformerMandatory {
            get {
                return isAutotransformerMandatory;
            }
        }
        
        public static string AutotransformerPrefix {
            get {
                return _autotransformerPrefix;
            }
        }
        
        public virtual TransformerFunctionKind Function {
            get {
                return this.cim_function.GetValueOrDefault();
            }
            set {
                this.cim_function = value;
            }
        }
        
        public virtual bool FunctionHasValue {
            get {
                return this.cim_function != null;
            }
        }
        
        public static bool IsFunctionMandatory {
            get {
                return isFunctionMandatory;
            }
        }
        
        public static string FunctionPrefix {
            get {
                return _functionPrefix;
            }
        }
    }
}
