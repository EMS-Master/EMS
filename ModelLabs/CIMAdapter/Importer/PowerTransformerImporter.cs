using System;
using System.Collections.Generic;
using CIM.Model;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter.Communication;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using FTN.ServiceContracts;
using FTN.ServiceContracts.ServiceFabricProxy;

namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	/// <summary>
	/// PowerTransformerImporter
	/// </summary>
	public class PowerTransformerImporter
	{
		/// <summary> Singleton </summary>
		private static PowerTransformerImporter ptImporter = null;
		private static object singletoneLock = new object();

		private ConcreteModel concreteModel;
		private Delta delta;
		private ImportHelper importHelper;
		private TransformAndLoadReport report;
        private NmsClient nmsCli = new NmsClient("NmsClientEndpoint");

		#region Properties
		public static PowerTransformerImporter Instance
		{
			get
			{
				if (ptImporter == null)
				{
					lock (singletoneLock)
					{
						if (ptImporter == null)
						{
							ptImporter = new PowerTransformerImporter();
							ptImporter.Reset();
						}
					}
				}
				return ptImporter;
			}
		}

		public Delta NMSDelta
		{
			get 
			{
				return delta;
			}
		}
		#endregion Properties


		public void Reset()
		{
			concreteModel = null;
			delta = new Delta();
			importHelper = new ImportHelper();
			report = null;
		}

		public TransformAndLoadReport CreateNMSDelta(ConcreteModel cimConcreteModel)
		{
			LogManager.Log("Importing PowerTransformer Elements...", LogLevel.Info);
			report = new TransformAndLoadReport();
			concreteModel = cimConcreteModel;
			delta.ClearDeltaOperations();

			if ((concreteModel != null) && (concreteModel.ModelMap != null))
			{
				try
				{
					// convert into DMS elements
					ConvertModelAndPopulateDelta();
				}
				catch (Exception ex)
				{
					string message = string.Format("{0} - ERROR in data import - {1}", DateTime.Now, ex.Message);
					LogManager.Log(message);
					report.Report.AppendLine(ex.Message);
					report.Success = false;
				}
			}
			LogManager.Log("Importing PowerTransformer Elements - END.", LogLevel.Info);
			return report;
		}

		/// <summary>
		/// Method performs conversion of network elements from CIM based concrete model into DMS model.
		/// </summary>
		private void ConvertModelAndPopulateDelta()
		{
			LogManager.Log("Loading elements and creating delta...", LogLevel.Info);

            //// import all concrete model types (DMSType enum)
            //ImportBaseVoltages();
            //ImportLocations();
            //ImportPowerTransformers();
            //ImportTransformerWindings();
            //ImportWindingTests();

            
            ImportSubstation();
            ImportGenerator();
            ImportEnergyConsumer();
            ImportAnalog();
            ImportDiscrete();
            ImportGeographicalRegion();

            LogManager.Log("Loading elements and creating delta completed.", LogLevel.Info);
		}

        #region Import
        private void ImportDiscrete()
        {
            SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("FTN.Discrete");
            if (cimBaseVoltages != null)
            {
                foreach (KeyValuePair<string, object> cimBaseVoltagePair in cimBaseVoltages)
                {
                    FTN.Discrete cimBaseVoltage = cimBaseVoltagePair.Value as FTN.Discrete;

                    ResourceDescription rd = CreateDiscreteResourceDescription(cimBaseVoltage);
                    if (rd != null)
                    {
                        if (ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id) > 0)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Update, rd, true);
                            report.Report.Append("EnergyConsumer ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Discrete ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                    }
                    else
                    {
                        report.Report.Append("Discrete ID = ").Append(cimBaseVoltage.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateDiscreteResourceDescription(FTN.Discrete discrete)
        {
            ResourceDescription rd = null;
            if (discrete != null)
            {
                long gid = 0;

                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 2;
                string message = string.Empty;
                bool contains = false;

                ModelCode modelCodeDiscrete = ModelCode.DISCRETE;
                List<ModelCode> properties = new List<ModelCode>();
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                List<ResourceDescription> retList = new List<ResourceDescription>();

                properties = modelResourcesDesc.GetAllPropertyIds(modelCodeDiscrete);
                iteratorId = nmsCli.GetExtentValues(modelCodeDiscrete, properties);
                resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = nmsCli.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);
                }
                nmsCli.IteratorClose(iteratorId);

                foreach (ResourceDescription res in retList)
                {
                    foreach (Property pr in res.Properties)
                    {
                        if (pr.PropertyValue.StringValue.Equals(discrete.MRID))
                        {
                            contains = true;
                            gid = res.Id;
                        }
                    }
                }

                if (!contains)
                {
                    gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.DISCRETE, importHelper.CheckOutIndexForDMSType(DMSType.DISCRETE));
                }
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(discrete.ID, gid);

                PowerTransformerConverter.PopulateDiscreteProperties(discrete, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportAnalog()
        {
            SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("FTN.Analog");
            if (cimBaseVoltages != null)
            {
                foreach (KeyValuePair<string, object> cimBaseVoltagePair in cimBaseVoltages)
                {
                    FTN.Analog cimBaseVoltage = cimBaseVoltagePair.Value as FTN.Analog;

                    ResourceDescription rd = CreateAnalogResourceDescription(cimBaseVoltage);
                    if (rd != null)
                    {
                        if (ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id) > 0)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Update, rd, true);
                            report.Report.Append("Analog ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());

                        }
                        else
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Analog ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                    }
                    else
                    {
                        report.Report.Append("Analog ID = ").Append(cimBaseVoltage.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateAnalogResourceDescription(FTN.Analog analog)
        {
            ResourceDescription rd = null;
            if (analog != null)
            {
                long gid = 0;

                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 2;
                string message = string.Empty;
                bool contains = false;

                ModelCode modelCodeAnalog = ModelCode.ANALOG;
                List<ModelCode> properties = new List<ModelCode>();
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                List<ResourceDescription> retList = new List<ResourceDescription>();

                properties = modelResourcesDesc.GetAllPropertyIds(modelCodeAnalog);
                iteratorId = nmsCli.GetExtentValues(modelCodeAnalog, properties);
                resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = nmsCli.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);
                }
                nmsCli.IteratorClose(iteratorId);

                foreach (ResourceDescription res in retList)
                {
                    foreach (Property pr in res.Properties)
                    {
                        if (pr.PropertyValue.StringValue.Equals(analog.MRID))
                        {
                            contains = true;
                            gid = res.Id;
                        }
                    }
                }

                if (!contains)
                {
                    gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ANALOG, importHelper.CheckOutIndexForDMSType(DMSType.ANALOG));
                }
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(analog.ID, gid);

                PowerTransformerConverter.PopulateAnalogProperties(analog, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportGenerator()
        {
            SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("FTN.Generator");
            if (cimBaseVoltages != null)
            {
                foreach (KeyValuePair<string, object> cimBaseVoltagePair in cimBaseVoltages)
                {
                    FTN.Generator cimBaseVoltage = cimBaseVoltagePair.Value as FTN.Generator;

                    ResourceDescription rd = CreateGeneratorResourceDescription(cimBaseVoltage);
                    if (rd != null)
                    {
                        if (ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id) > 0)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Update, rd, true);
                            report.Report.Append("Generator ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                        else
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Generator ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());

                        }
                    }
                    else
                    {
                        report.Report.Append("Generator ID = ").Append(cimBaseVoltage.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateGeneratorResourceDescription(FTN.Generator generator)
        {
            ResourceDescription rd = null;
            if (generator != null)
            {
                long gid = 0;

                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 2;
                string message = string.Empty;
                bool contains = false;

                ModelCode modelCodeGenerator = ModelCode.GENERATOR;
                List<ModelCode> properties = new List<ModelCode>();
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                List<ResourceDescription> retList = new List<ResourceDescription>();

                properties = modelResourcesDesc.GetAllPropertyIds(modelCodeGenerator);
                iteratorId = nmsCli.GetExtentValues(modelCodeGenerator, properties);
                resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = nmsCli.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);
                }
                nmsCli.IteratorClose(iteratorId);

                foreach (ResourceDescription res in retList)
                {
                    foreach (Property pr in res.Properties)
                    {
                        if (pr.PropertyValue.StringValue.Equals(generator.MRID))
                        {
                            contains = true;
                            gid = res.Id;
                        }
                    }
                }

                if (!contains)
                {
                    gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.GENERATOR, importHelper.CheckOutIndexForDMSType(DMSType.GENERATOR));
                }
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(generator.ID, gid);

                PowerTransformerConverter.PopulateGeneratorProperties(generator, rd, importHelper, report);
            }
            return rd;
        }

		private void ImportEnergyConsumer()
		{
			SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("FTN.EnergyConsumer");
			if (cimBaseVoltages != null)
			{
				foreach (KeyValuePair<string, object> cimBaseVoltagePair in cimBaseVoltages)
				{
					FTN.EnergyConsumer energyConsumer = cimBaseVoltagePair.Value as FTN.EnergyConsumer;

					ResourceDescription rd = CreateEnergyConsumerResourceDescription(energyConsumer);
					if (rd != null)
					{
                        if (ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id) > 0)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Update, rd, true);
                            report.Report.Append("EnergyConsumer ID = ").Append(energyConsumer.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());

                        }
                        else
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("EnergyConsumer ID = ").Append(energyConsumer.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                    }
					else
					{
						report.Report.Append("EnergyConsumer ID = ").Append(energyConsumer.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateEnergyConsumerResourceDescription(FTN.EnergyConsumer energyConsumer)
		{

            ResourceDescription rd = null;
            if (energyConsumer != null)
            {
                long gid = 0;

                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 2;
                string message = string.Empty;
                bool contains = false;

                ModelCode modelCodeEnergyConsumer = ModelCode.ENERGY_CONSUMER;
                List<ModelCode> properties = new List<ModelCode>();
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                List<ResourceDescription> retList = new List<ResourceDescription>();

                properties = modelResourcesDesc.GetAllPropertyIds(modelCodeEnergyConsumer);
                iteratorId = nmsCli.GetExtentValues(modelCodeEnergyConsumer, properties);
                resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = nmsCli.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);
                }
                nmsCli.IteratorClose(iteratorId);

                foreach (ResourceDescription res in retList)
                {
                    foreach (Property pr in res.Properties)
                    {
                        if (pr.PropertyValue.StringValue.Equals(energyConsumer.MRID))
                        {
                            contains = true;
                            gid = res.Id;
                        }
                    }
                }

                if (!contains)
                {
                    gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ENERGY_CONSUMER, importHelper.CheckOutIndexForDMSType(DMSType.ENERGY_CONSUMER));
                }
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(energyConsumer.ID, gid);

                PowerTransformerConverter.PopulateEnergyConsumerProperties(energyConsumer, rd, importHelper, report);
            }
            return rd;
            
		}


		private void ImportGeographicalRegion()
        {
            SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("FTN.GeographicalRegion");
            if (cimBaseVoltages != null)
            {
                foreach (KeyValuePair<string, object> cimBaseVoltagePair in cimBaseVoltages)
                {
                    FTN.GeographicalRegion cimBaseVoltage = cimBaseVoltagePair.Value as FTN.GeographicalRegion;

                    ResourceDescription rd = CreateGeographicalRegionResourceDescription(cimBaseVoltage);
                    if (rd != null)
                    {
                        if (ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id) > 0)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Update, rd, true);
                            report.Report.Append("GeographicalRegion ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());

                        }
                        else
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("GeographicalRegion ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                    }
                    else
                    {
                        report.Report.Append("GeographicalRegion ID = ").Append(cimBaseVoltage.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateGeographicalRegionResourceDescription(FTN.GeographicalRegion geographical)
        {
            ResourceDescription rd = null;
            if (geographical != null)
            {
                long gid = 0;

                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 2;
                string message = string.Empty;
                bool contains = false;

                ModelCode modelCodeGeo = ModelCode.GEOGRAFICAL_REGION;
                List<ModelCode> properties = new List<ModelCode>();
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                List<ResourceDescription> retList = new List<ResourceDescription>();

                properties = modelResourcesDesc.GetAllPropertyIds(modelCodeGeo);
                iteratorId = nmsCli.GetExtentValues(modelCodeGeo, properties);
                resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = nmsCli.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);
                }
                nmsCli.IteratorClose(iteratorId);

                foreach (ResourceDescription res in retList)
                {
                    foreach (Property pr in res.Properties)
                    {
                        if (pr.PropertyValue.StringValue.Equals(geographical.MRID))
                        {
                            contains = true;
                            gid = res.Id;
                        }
                    }
                }

                if (!contains)
                {
                    gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.GEOGRAFICAL_REGION, importHelper.CheckOutIndexForDMSType(DMSType.GEOGRAFICAL_REGION));
                }
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(geographical.ID, gid);

                PowerTransformerConverter.PopulateGeographicalRegionProperties(geographical, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportSubstation()
        {
            SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("FTN.Substation");
            if (cimBaseVoltages != null)
            {
                foreach (KeyValuePair<string, object> cimBaseVoltagePair in cimBaseVoltages)
                {
                    FTN.Substation cimBaseVoltage = cimBaseVoltagePair.Value as FTN.Substation;

                    ResourceDescription rd = CreateSubstationResourceDescription(cimBaseVoltage);
                    if (rd != null)
                    {
                        if (ModelCodeHelper.ExtractEntityIdFromGlobalId(rd.Id) > 0)
                        {
                            delta.AddDeltaOperation(DeltaOpType.Update, rd, true);
                            report.Report.Append("Substation ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());

                        }
                        else
                        {
                            delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                            report.Report.Append("Substation ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                        }
                    }
                    else
                    {
                        report.Report.Append("Substation ID = ").Append(cimBaseVoltage.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateSubstationResourceDescription(FTN.Substation substation)
        {
            ResourceDescription rd = null;
            if (substation != null)
            {
                long gid = 0;

                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 2;
                string message = string.Empty;
                bool contains = false;

                ModelCode modelCodeSubstation = ModelCode.SUBSTATION;
                List<ModelCode> properties = new List<ModelCode>();
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
                List<ResourceDescription> retList = new List<ResourceDescription>();

                properties = modelResourcesDesc.GetAllPropertyIds(modelCodeSubstation);
                iteratorId = nmsCli.GetExtentValues(modelCodeSubstation, properties);
                resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = nmsCli.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = nmsCli.IteratorResourcesLeft(iteratorId);
                }
                nmsCli.IteratorClose(iteratorId);

                foreach (ResourceDescription res in retList)
                {
                    foreach (Property pr in res.Properties)
                    {
                        if (pr.PropertyValue.StringValue.Equals(substation.MRID))
                        {
                            contains = true;
                            gid = res.Id;
                        }
                    }
                }

                if (!contains)
                {
                    gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SUBSTATION, importHelper.CheckOutIndexForDMSType(DMSType.SUBSTATION));
                }
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(substation.ID, gid);

                PowerTransformerConverter.PopulateSubstationProperties(substation, rd, importHelper, report);
            }
            return rd;
        }
		#endregion Import
	}
}

