namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	using FTN.Common;

	/// <summary>
	/// PowerTransformerConverter has methods for populating
	/// ResourceDescription objects using PowerTransformerCIMProfile_Labs objects.
	/// </summary>
	public static class PowerTransformerConverter
	{

		#region Populate ResourceDescription
		public static void PopulateIdentifiedObjectProperties(FTN.IdentifiedObject cimIdentifiedObject, ResourceDescription rd)
		{
			if ((cimIdentifiedObject != null) && (rd != null))
			{
				if (cimIdentifiedObject.MRIDHasValue)
				{
					rd.AddProperty(new Property(ModelCode.IDOBJ_MRID, cimIdentifiedObject.MRID));
				}
				if (cimIdentifiedObject.NameHasValue)
				{
					rd.AddProperty(new Property(ModelCode.IDOBJ_NAME, cimIdentifiedObject.Name));
				}
				if (cimIdentifiedObject.AliasNameHasValue)
				{
					rd.AddProperty(new Property(ModelCode.IDOBJ_ALIASNAME, cimIdentifiedObject.AliasName));
				}
			}
		}

        public static void PopulateDiscreteProperties(FTN.Discrete cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateMeasurementProperties(cimIdentifiedObject, rd, importHelper, report);

                if (cimIdentifiedObject.MaxValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_MAX_VALUE, cimIdentifiedObject.MaxValue));
                }
                if (cimIdentifiedObject.MinValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_MIN_VALUE, cimIdentifiedObject.MinValue));
                }
                if (cimIdentifiedObject.NormalValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_NORMAL_VALUE, cimIdentifiedObject.NormalValue));
                }
            }
        }

        public static void PopulateAnalogProperties(FTN.Analog cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateMeasurementProperties(cimIdentifiedObject, rd, importHelper, report);

                if (cimIdentifiedObject.MaxValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_MAX_VALUE, cimIdentifiedObject.MaxValue));
                }
                if (cimIdentifiedObject.MinValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_MIN_VALUE, cimIdentifiedObject.MinValue));
                }
                if (cimIdentifiedObject.NormalValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_NORMAL_VALUE, cimIdentifiedObject.NormalValue));
                }
            }
        }

        public static void PopulateEnergyConsumerProperties(FTN.EnergyConsumer cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConductingEquipmentProperties(cimIdentifiedObject, rd, importHelper, report);

                if (cimIdentifiedObject.CurrentPowerHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGY_CONSUMER_CURRENT_POWER, cimIdentifiedObject.CurrentPower));
                }
                if (cimIdentifiedObject.PfixedHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGY_CONSUMER_PFIXED, cimIdentifiedObject.Pfixed));
                }
            }
        }

        public static void PopulateGeneratorProperties(FTN.Generator cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateRotatingMachineProperties(cimIdentifiedObject, rd, importHelper, report);

                if (cimIdentifiedObject.MaxQHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_MAX_Q, cimIdentifiedObject.MaxQ));
                }
                if (cimIdentifiedObject.MinQHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_MIN_Q, cimIdentifiedObject.MinQ));
                }
                if (cimIdentifiedObject.GeneratorTypeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.GENERATOR_TYPE, (short)GetDMSGeneratorType(cimIdentifiedObject.GeneratorType)));
                }
            }
        }

        public static void PopulateConductingEquipmentProperties(FTN.ConductingEquipment cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateEquipmentProperties(cimIdentifiedObject, rd, importHelper, report);
            }
        }

        public static void PopulateSubstationProperties(FTN.Substation cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateEquipmentContainerProperties(cimIdentifiedObject, rd, importHelper, report);

            }
        }

        public static void PopulateRegulatingConductingEqProperties(FTN.RegulatingCondEq cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConductingEquipmentProperties(cimIdentifiedObject, rd, importHelper, report);

            }
        }
        public static void PopulateGeographicalRegionProperties(FTN.GeographicalRegion cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimIdentifiedObject, rd);
            }
        }
        public static void PopulateConnectivityNodeContainerProperties(FTN.ConnectivityNodeContainer cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulatePowerSystemResourceProperties(cimIdentifiedObject, rd, importHelper, report);
            }
        }

        public static void PopulateRotatingMachineProperties(FTN.RotatingMachine cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateRegulatingConductingEqProperties(cimIdentifiedObject, rd, importHelper, report);

                if (cimIdentifiedObject.RatedSHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ROTATING_MACHINE_RATED_S, cimIdentifiedObject.RatedS));
                }                
            }
        }

        public static void PopulateBatteryStorageProperties(FTN.BatteryStorage cimIdentifiedObject, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateEnergyConsumerProperties(cimIdentifiedObject, rd, importHelper, report);

                if (cimIdentifiedObject.MaxPowerHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.BATTERY_STORAGE_MAX_POWER, cimIdentifiedObject.MaxPower));
                }
                if (cimIdentifiedObject.MinCapasityHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.BATTERY_STORAGE_MIN_CAPACITY, cimIdentifiedObject.MinCapasity));
                }
            }
        }

        public static void PopulatePowerSystemResourceProperties(FTN.PowerSystemResource cimBaseVoltage, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimBaseVoltage != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimBaseVoltage, rd);                
            }
        }

        public static void PopulateEquipmentContainerProperties(FTN.EquipmentContainer cimBaseVoltage, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimBaseVoltage != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateConnectivityNodeContainerProperties(cimBaseVoltage, rd, importHelper, report);
            }
        }


        public static void PopulateEquipmentProperties(FTN.Equipment cimConductingEquipment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimConductingEquipment != null) && (rd != null))
            {
                PowerTransformerConverter.PopulatePowerSystemResourceProperties(cimConductingEquipment, rd, importHelper, report);                
                if (cimConductingEquipment.EquipmentContainerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimConductingEquipment.EquipmentContainer.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimConductingEquipment.GetType().ToString()).Append(" rdfID = \"").Append(cimConductingEquipment.ID);
                        report.Report.Append("\" - Failed to set reference to EquipmentContainer: rdfID \"").Append(cimConductingEquipment.EquipmentContainer.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.EQUIPMENT_EQUIPMENT_CONTAINER, gid));
                }
            }
        }

        public static void PopulateMeasurementProperties(FTN.Measurement cimConductingEquipment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimConductingEquipment != null) && (rd != null))
            {
                PowerTransformerConverter.PopulateIdentifiedObjectProperties(cimConductingEquipment, rd);
                if (cimConductingEquipment.PowerSystemResourceHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimConductingEquipment.PowerSystemResource.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimConductingEquipment.GetType().ToString()).Append(" rdfID = \"").Append(cimConductingEquipment.ID);
                        report.Report.Append("\" - Failed to set reference to PowerSystemResource: rdfID \"").Append(cimConductingEquipment.PowerSystemResource.ID).AppendLine(" \" is not mapped to GID!");
                    }
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_POWER_SYS_RESOURCE, gid));
                }
            }
        }
                                 

		#endregion Populate ResourceDescription

		#region Enums convert	

        public static GeneratorType GetDMSGeneratorType(FTN.GeneratorType generatorType)
        {
            switch (generatorType)
            {
                case FTN.GeneratorType.wind:
                    return GeneratorType.Wind;
                case FTN.GeneratorType.solar:
                    return GeneratorType.Sollar;
                case FTN.GeneratorType.oil:
                    return GeneratorType.Oil;
                case FTN.GeneratorType.hydro:
                    return GeneratorType.Hydro;
                case FTN.GeneratorType.gas:
                    return GeneratorType.Gas;
                case FTN.GeneratorType.coal:
                    return GeneratorType.Coal;

                default: return GeneratorType.Unknown;

            }
        }

        public static Direction GetDMSDirection(FTN.Direction direction)
        {
            switch (direction)
            {
                case FTN.Direction.read:
                    return Direction.Read;
                case FTN.Direction.write:
                    return Direction.Write;
                case FTN.Direction.readWrite:
                    return Direction.ReadWrite;

                default: return Direction.Unknown;

            }
        }
        public static MeasurementType GetDMSMeasurementType(FTN.MeasurementType measurementType)
        {
            switch (measurementType)
            {
                case FTN.MeasurementType.voltage:
                    return MeasurementType.Voltage;
                case FTN.MeasurementType.activePower:
                    return MeasurementType.ActivePower;
                
                default: return MeasurementType.Unknown;

            }
        }


     
		#endregion Enums convert
	}
}
