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
    
    
    /// Set of spatial coordinates that determine a point. Use a single position point instance to desribe a point-oriented location. Use a sequence of position points to describe a line-oriented object (physical location of non-point oriented objects like cables or lines), or area of an object (like a substation or a geographical zone - in this case, have first and last position point with the same values).
    public class PositionPoint : IDClass {
        
        /// Coordinate system in which the coordinates of this position point are expressed.
        private CoordinateSystem cim_CoordinateSystem;
        
        private const bool isCoordinateSystemMandatory = true;
        
        private const string _CoordinateSystemPrefix = "cim";
        
        /// For display purposes, rotationAngle will define how the device will be drawn.
        private System.Single? cim_rotationAngle;
        
        private const bool isRotationAngleMandatory = false;
        
        private const string _rotationAnglePrefix = "ftn";
        
        /// scaleFactor can be used to override default symbol sizes.
        private System.Single? cim_scaleFactor;
        
        private const bool isScaleFactorMandatory = false;
        
        private const string _scaleFactorPrefix = "ftn";
        
        /// Zero-relative sequence number of this point within a series of points.
        private System.Int32? cim_sequenceNumber;
        
        private const bool isSequenceNumberMandatory = true;
        
        private const string _sequenceNumberPrefix = "cim";
        
        /// X axis position.
        private string cim_xPosition;
        
        private const bool isXPositionMandatory = true;
        
        private const string _xPositionPrefix = "cim";
        
        /// Y axis position.
        private string cim_yPosition;
        
        private const bool isYPositionMandatory = true;
        
        private const string _yPositionPrefix = "cim";
        
        public virtual CoordinateSystem CoordinateSystem {
            get {
                return this.cim_CoordinateSystem;
            }
            set {
                this.cim_CoordinateSystem = value;
            }
        }
        
        public virtual bool CoordinateSystemHasValue {
            get {
                return this.cim_CoordinateSystem != null;
            }
        }
        
        public static bool IsCoordinateSystemMandatory {
            get {
                return isCoordinateSystemMandatory;
            }
        }
        
        public static string CoordinateSystemPrefix {
            get {
                return _CoordinateSystemPrefix;
            }
        }
        
        public virtual float RotationAngle {
            get {
                return this.cim_rotationAngle.GetValueOrDefault();
            }
            set {
                this.cim_rotationAngle = value;
            }
        }
        
        public virtual bool RotationAngleHasValue {
            get {
                return this.cim_rotationAngle != null;
            }
        }
        
        public static bool IsRotationAngleMandatory {
            get {
                return isRotationAngleMandatory;
            }
        }
        
        public static string RotationAnglePrefix {
            get {
                return _rotationAnglePrefix;
            }
        }
        
        public virtual float ScaleFactor {
            get {
                return this.cim_scaleFactor.GetValueOrDefault();
            }
            set {
                this.cim_scaleFactor = value;
            }
        }
        
        public virtual bool ScaleFactorHasValue {
            get {
                return this.cim_scaleFactor != null;
            }
        }
        
        public static bool IsScaleFactorMandatory {
            get {
                return isScaleFactorMandatory;
            }
        }
        
        public static string ScaleFactorPrefix {
            get {
                return _scaleFactorPrefix;
            }
        }
        
        public virtual int SequenceNumber {
            get {
                return this.cim_sequenceNumber.GetValueOrDefault();
            }
            set {
                this.cim_sequenceNumber = value;
            }
        }
        
        public virtual bool SequenceNumberHasValue {
            get {
                return this.cim_sequenceNumber != null;
            }
        }
        
        public static bool IsSequenceNumberMandatory {
            get {
                return isSequenceNumberMandatory;
            }
        }
        
        public static string SequenceNumberPrefix {
            get {
                return _sequenceNumberPrefix;
            }
        }
        
        public virtual string XPosition {
            get {
                return this.cim_xPosition;
            }
            set {
                this.cim_xPosition = value;
            }
        }
        
        public virtual bool XPositionHasValue {
            get {
                return this.cim_xPosition != null;
            }
        }
        
        public static bool IsXPositionMandatory {
            get {
                return isXPositionMandatory;
            }
        }
        
        public static string XPositionPrefix {
            get {
                return _xPositionPrefix;
            }
        }
        
        public virtual string YPosition {
            get {
                return this.cim_yPosition;
            }
            set {
                this.cim_yPosition = value;
            }
        }
        
        public virtual bool YPositionHasValue {
            get {
                return this.cim_yPosition != null;
            }
        }
        
        public static bool IsYPositionMandatory {
            get {
                return isYPositionMandatory;
            }
        }
        
        public static string YPositionPrefix {
            get {
                return _yPositionPrefix;
            }
        }
    }
}