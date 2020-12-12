using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using System.Xml;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Core;
using FTN.Services.NetworkModelService.DataModel.Wires;
using FTN.Services.NetworkModelService.NmsModel;
using TransactionContract;

namespace FTN.Services.NetworkModelService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class NetworkModel : ITransactionContract
	{
		/// <summary>
		/// Dictionaru which contains all data: Key - DMSType, Value - Container
		/// </summary>
		private static Dictionary<DMSType, Container> networkDataModel;
        private Dictionary<DMSType, Container> networkDataModelCopy;
        private ITransactionCallback transactionCallback;

        private readonly string MODE = "DATABASE";

        private static object obj = new object();
        private Delta deltaToCommit;

        private int deltaCount = 0;
        /// <summary>
        /// ModelResourceDesc class contains metadata of the model
        /// </summary>
        private ModelResourcesDesc resourcesDescs;
	
		/// <summary>
		/// Initializes a new instance of the Model class.
		/// </summary>
		public NetworkModel()
		{
			networkDataModel = new Dictionary<DMSType, Container>();
			networkDataModelCopy = new Dictionary<DMSType, Container>();
			resourcesDescs = new ModelResourcesDesc();			
			Initialize();
		}
	
		#region Find
		
		public bool EntityExists(long globalId)
		{
			DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

		    if (ContainerExists(type))
		    {
				Container container = GetContainer(type);

				if (container.EntityExists(globalId))
				{
					return true;
				}
		    }

		    return false;
		}

		public IdentifiedObject GetEntity(long globalId)
		{
			if (EntityExists(globalId))
			{
				DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
				IdentifiedObject io = GetContainer(type).GetEntity(globalId);

				return io;
			}
			else
			{
				string message = string.Format("Entity  (GID = 0x{0:x16}) does not exist.", globalId);
				throw new Exception(message);
			}
		}


		/// <summary>
		/// Checks if container exists in model.
		/// </summary>
		/// <param name="type">Type of container.</param>
		/// <returns>True if container exists, otherwise FALSE.</returns>
		private bool ContainerExists(DMSType type)
		{
			if (networkDataModel.ContainsKey(type))
			{
				return true;
			}
			
			return false;			
		}

		/// <summary>
		/// Gets container of specified type.
		/// </summary>
		/// <param name="type">Type of container.</param>
		/// <returns>Container for specified local id</returns>
		private Container GetContainer(DMSType type)
		{
			if (ContainerExists(type))
			{
				return networkDataModel[type];
			}
			else
			{
				string message = string.Format("Container does not exist for type {0}.", type);
				throw new Exception(message);
			}
			
		}

		#endregion Find

		#region GDA query

		/// <summary>
		/// Gets resource description for entity requested by globalId.
		/// </summary>
		/// <param name="globalId">Id of the entity</param>
		/// <param name="properties">List of requested properties</param>		
		/// <returns>Resource description of the specified entity</returns>
		public ResourceDescription GetValues(long globalId, List<ModelCode> properties)
		{
			CommonTrace.WriteTrace(CommonTrace.TraceVerbose, String.Format("Getting values for GID = 0x{0:x16}.", globalId));

			try
			{
				IdentifiedObject io = GetEntity(globalId);

				ResourceDescription rd = new ResourceDescription(globalId);

				Property property = null;

				// insert specified properties
				foreach (ModelCode propId in properties)
				{
					property = new Property(propId);
					io.GetProperty(property);
					rd.AddProperty(property);
				}

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, String.Format("Getting values for GID = 0x{0:x16} succedded.", globalId));

				return rd;
			}			
			catch (Exception ex)
			{
				string message = string.Format("Failed to get values for entity with GID = 0x{0:x16}. {1}", globalId, ex.Message);				
				throw new Exception(message);
			}
		}

		/// <summary>
		/// Gets resource iterator that holds descriptions for all entities of the specified type.
		/// </summary>		
		/// <param name="type">Type of entity that is requested</param>
		/// <param name="properties">List of requested properties</param>		
		/// <returns>Resource iterator for the requested entities</returns>
		public ResourceIterator GetExtentValues(ModelCode entityType, List<ModelCode> properties)
		{			
			CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Getting extent values for entity type = {0} .", entityType);

			try
			{
				List<long> globalIds = new List<long>();
				Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>();
		
				DMSType entityDmsType = ModelCodeHelper.GetTypeFromModelCode(entityType);
				
				if (ContainerExists(entityDmsType))
				{
					Container container = GetContainer(entityDmsType);
					globalIds = container.GetEntitiesGlobalIds();					
					class2PropertyIDs.Add(entityDmsType, properties);					
				}

				ResourceIterator ri = new ResourceIterator(globalIds, class2PropertyIDs);

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Getting extent values for entity type = {0} succedded.", entityType);

				return ri;
			}		
			catch (Exception ex)
			{
				string message = string.Format("Failed to get extent values for entity type = {0}. {1}", entityType, ex.Message);				
				throw new Exception(message);
			}			
		}
		
		/// <summary>
		/// Gets resource iterator that holds descriptions for all entities related to specified source.
		/// </summary>
		/// <param name="contextId">Context Id</param>
		/// <param name="properties">List of requested properties</param>
		/// <param name="association">Relation between source and entities that should be returned</param>
		/// <param name="source">Id of entity that is start for association search</param>
		/// <param name="typeOfQuery">Query type choice(global or local)</param>
		/// <returns>Resource iterator for the requested entities</returns>
		public ResourceIterator GetRelatedValues(long source, List<ModelCode> properties, Association association)
		{
			CommonTrace.WriteTrace(CommonTrace.TraceVerbose, String.Format("Getting related values for source = 0x{0:x16}.", source));
		
			try
			{
				List<long> relatedGids = ApplyAssocioationOnSource(source, association);
			
				
				Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>();

				foreach (long relatedGid in relatedGids)
				{
					DMSType entityDmsType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(relatedGid);

					if (!class2PropertyIDs.ContainsKey(entityDmsType))
					{
						class2PropertyIDs.Add(entityDmsType, properties);
					}
				}

				ResourceIterator ri = new ResourceIterator(relatedGids, class2PropertyIDs);

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, String.Format("Getting related values for source = 0x{0:x16} succeeded.", source));

				return ri;
			}
			catch (Exception ex)
			{
				string message = String.Format("Failed to get related values for source GID = 0x{0:x16}. {1}.", source, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		#endregion GDA query	

		public UpdateResult ApplyDelta(Delta delta)
		{
			bool applyingStarted = false;
			UpdateResult updateResult = new UpdateResult();

			try
			{
				CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Applying  delta to network model.");

				Dictionary<short, int> typesCounters = GetCounters();
				Dictionary<long, long> globalIdPairs = new Dictionary<long, long>();
				delta.FixNegativeToPositiveIds(ref typesCounters, ref globalIdPairs);
				updateResult.GlobalIdPairs = globalIdPairs;
				delta.SortOperations();

				applyingStarted = true;

				foreach (ResourceDescription rd in delta.InsertOperations)
				{
					InsertEntity(rd);
				}

				foreach (ResourceDescription rd in delta.UpdateOperations)
				{
					UpdateEntity(rd);
				}

				foreach (ResourceDescription rd in delta.DeleteOperations)
				{
					DeleteEntity(rd);
				}				 				

			}
			catch (Exception ex)
			{
				string message = string.Format("Applying delta to network model failed. {0}.", ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);

				updateResult.Result = ResultType.Failed;
				updateResult.Message = message;
			}
			finally
			{
				//if (applyingStarted)
				//{
				//	SaveDelta(delta);
				//}

                deltaToCommit = delta;


                if (updateResult.Result == ResultType.Succeeded)
				{
					string mesage = "Applying delta to network model successfully finished.";
					CommonTrace.WriteTrace(CommonTrace.TraceInfo, mesage);
					updateResult.Message = mesage;
				}				
			}

			return updateResult;
		}

        /// <summary>
        /// Inserts entity into the network model.
        /// </summary>
        /// <param name="rd">Description of the resource that should be inserted</param>        
		private void InsertEntity(ResourceDescription rd)
		{
			if (rd == null)
			{
				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Insert entity is not done because update operation is empty.");
				return;
			}			

			long globalId = rd.Id;

			CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Inserting entity with GID ({0:x16}).", globalId);

			// check if mapping for specified global id already exists			
			if (this.EntityCopyExists(globalId))
			{
				string message = String.Format("Failed to insert entity because entity with specified GID ({0:x16}) already exists in network model.", globalId);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}

			try
			{
				// find type
				DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

				Container container = null;

				// get container or create container 
				if (ContainerCopyExists(type))
				{
					container = GetContainerCopy(type);
				}
				else
				{
					container = new Container();
					networkDataModelCopy.Add(type, container);
				}

				// create entity and add it to container
				IdentifiedObject io = container.CreateEntity(globalId);

				// apply properties on created entity
				if (rd.Properties != null)
				{
					foreach (Property property in  rd.Properties)
					{						
						// globalId must not be set as property
						if (property.Id == ModelCode.IDOBJ_GID)
						{
							continue;
						}

						if (property.Type == PropertyType.Reference)
						{
							// if property is a reference to another entity 
							long targetGlobalId = property.AsReference();

							if (targetGlobalId != 0)
							{

								if (!EntityCopyExists(targetGlobalId))
								{
									string message = string.Format("Failed to get target entity with GID: 0x{0:X16}. {1}", targetGlobalId);
									throw new Exception(message);
								}

								// get referenced entity for update
								IdentifiedObject targetEntity = GetEntityCopy(targetGlobalId);
								targetEntity.AddReference(property.Id, io.GlobalId);																	
							}

							io.SetProperty(property);
						}
						else
						{
							io.SetProperty(property);
						}						
					}
				}

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Inserting entity with GID ({0:x16}) successfully finished.", globalId);
			}			
			catch (Exception ex)
			{
				string message = String.Format("Failed to insert entity (GID = 0x{0:x16}) into model. {1}", rd.Id, ex.Message);				
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}
		
		/// <summary>
		/// Updates entity in block model.
		/// </summary>
		/// <param name="rd">Description of the resource that should be updated</param>		
		private void UpdateEntity(ResourceDescription rd)
		{
			if (rd == null || rd.Properties == null && rd.Properties.Count == 0)
			{	
				CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Update entity is not done because update operation is empty.");			
				return;
			}
			
			try
			{
					long globalId = rd.Id;

					CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Updating entity with GID ({0:x16}).", globalId);

					if (!this.EntityCopyExists(globalId))
					{
						string message = String.Format("Failed to update entity because entity with specified GID ({0:x16}) does not exist in network model.", globalId);
						CommonTrace.WriteTrace(CommonTrace.TraceError, message);
						throw new Exception(message);
					}

					IdentifiedObject io = GetEntityCopy(globalId);					

					// updating properties of entity
					foreach (Property property in rd.Properties)
					{
						if (property.Type == PropertyType.Reference)
						{
                            long oldTargetGlobalId = io.GetProperty(property.Id).AsReference();
                            
                            if (oldTargetGlobalId != 0)
                            {
                                IdentifiedObject oldTargetEntity = GetEntityCopy(oldTargetGlobalId);
                                oldTargetEntity.RemoveReference(property.Id, globalId);
                            }

							// updating reference of entity
							long targetGlobalId = property.AsReference();

							if (targetGlobalId != 0)
							{								
								if (!EntityCopyExists(targetGlobalId))
								{
									string message = string.Format("Failed to get target entity with GID: 0x{0:X16}.", targetGlobalId);
									throw new Exception(message);
								}

                                IdentifiedObject targetEntity = GetEntityCopy(targetGlobalId);
                                targetEntity.AddReference(property.Id, globalId);
							}

							// update value of the property in specified entity
							io.SetProperty(property);
						}
						else
						{
							// update value of the property in specified entity
							io.SetProperty(property);
						}							
					}

					CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Updating entity with GID ({0:x16}) successfully finished.", globalId);
			}						
			catch (Exception ex)
			{
				string message = String.Format("Failed to update entity (GID = 0x{0:x16}) in model. {1} ", rd.Id, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		/// <summary>
		/// Deletes resource from the netowrk model.
		/// </summary>
		/// <param name="rd">Description of the resource that should be deleted</param>		
		private void DeleteEntity(ResourceDescription rd)
		{
			if (rd == null)
			{
				CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Delete entity is not done because update operation is empty.");
				return;
			}

			try
			{
				long globalId = rd.Id;

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Deleting entity with GID ({0:x16}).", globalId);

				// check if entity exists
				if (!this.EntityExists(globalId))
				{
					string message = String.Format("Failed to delete entity because entity with specified GID ({0:x16}) does not exist in network model.", globalId);
					CommonTrace.WriteTrace(CommonTrace.TraceError, message);
					throw new Exception(message);
				}

				// get entity to be deleted
				IdentifiedObject io = GetEntity(globalId);

				// check if entity could be deleted (if it is not referenced by any other entity)
				if (io.IsReferenced)
				{
                    Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode,List<long>>();
                    io.GetReferences(references, TypeOfReference.Target);

                    StringBuilder sb = new StringBuilder();
                    
                    foreach(KeyValuePair<ModelCode, List<long>> kvp in references)
                    {
                        foreach (long referenceGlobalId in kvp.Value)
                        {
                            sb.AppendFormat("0x{0:x16}, ", referenceGlobalId);
                        }
                    }

					string message = String.Format("Failed to delete entity (GID = 0x{0:x16}) because it is referenced by entities with GIDs: {1}.", globalId, sb.ToString());
					CommonTrace.WriteTrace(CommonTrace.TraceError, message);
					throw new Exception(message);
				}
							
				// find property ids
				List<ModelCode> propertyIds = resourcesDescs.GetAllSettablePropertyIdsForEntityId(io.GlobalId);

				// remove references
				Property property = null;
				foreach (ModelCode propertyId in propertyIds)
				{
					PropertyType propertyType = Property.GetPropertyType(propertyId);

					if (propertyType == PropertyType.Reference)
					{
						property = io.GetProperty(propertyId);

						if (propertyType == PropertyType.Reference)
						{
							// get target entity and remove reference to another entity
							long targetGlobalId = property.AsReference();

							if (targetGlobalId != 0)
							{
								// get target entity
								IdentifiedObject targetEntity = GetEntity(targetGlobalId);

								// remove reference to another entity
								targetEntity.RemoveReference(propertyId, globalId);
							}
						}						                        
					}
				}

				// remove entity form netowrk model
				DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
				Container container = GetContainer(type);
				container.RemoveEntity(globalId);

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Deleting entity with GID ({0:x16}) successfully finished.", globalId);
			}			
			catch (Exception ex)
			{
				string message = String.Format("Failed to delete entity (GID = 0x{0:x16}) from model. {1}", rd.Id, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		/// <summary>
		/// Returns related gids with source according to the association 
		/// </summary>
		/// <param name="source">source id</param>		
		/// <param name="association">desinition of association</param>
		/// <returns>related gids</returns>
		private List<long> ApplyAssocioationOnSource(long source, Association association)
		{
			List<long> relatedGids = new List<long>();

			if (association == null)
			{
				association = new Association();
			}

			IdentifiedObject io = GetEntity(source);

			if (!io.HasProperty(association.PropertyId))
			{
				throw new Exception(string.Format("Entity with GID = 0x{0:x16} does not contain prperty with Id = {1}.", source, association.PropertyId));
			}

			Property propertyRef = null;
			if (Property.GetPropertyType(association.PropertyId) == PropertyType.Reference)
			{
				propertyRef = io.GetProperty(association.PropertyId);
				long relatedGidFromProperty = propertyRef.AsReference();

				if (relatedGidFromProperty != 0)
				{
					if (association.Type == 0 || (short)ModelCodeHelper.GetTypeFromModelCode(association.Type) == ModelCodeHelper.ExtractTypeFromGlobalId(relatedGidFromProperty))
					{
						relatedGids.Add(relatedGidFromProperty);
					}
				}
			}
			else if (Property.GetPropertyType(association.PropertyId) == PropertyType.ReferenceVector)
			{
				propertyRef = io.GetProperty(association.PropertyId);
				List<long> relatedGidsFromProperty = propertyRef.AsReferences();

				if (relatedGidsFromProperty != null)
				{
					foreach (long relatedGidFromProperty in relatedGidsFromProperty)
					{
						if (association.Type == 0 || (short)ModelCodeHelper.GetTypeFromModelCode(association.Type) == ModelCodeHelper.ExtractTypeFromGlobalId(relatedGidFromProperty))
						{
							relatedGids.Add(relatedGidFromProperty);
						}
					}
				}
			}
			else
			{
				throw new Exception(string.Format("Association propertyId = {0} is not reference or reference vector type.", association.PropertyId));
			}

			return relatedGids;
		}

		/// <summary>
		/// Writes delta to log
		/// </summary>
		/// <param name="delta">delta instance which will be logged</param>
		public static void TraceDelta(Delta delta)
		{
			try
			{
				StringWriter stringWriter = new StringWriter();
				XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
				xmlWriter.Formatting = Formatting.Indented;
				delta.ExportToXml(xmlWriter);
				xmlWriter.Flush();
				CommonTrace.WriteTrace(CommonTrace.TraceInfo, stringWriter.ToString());				
				xmlWriter.Close();
				stringWriter.Close();
			}
			catch (Exception ex)
			{
				CommonTrace.WriteTrace(CommonTrace.TraceError, "Failed to trace delta with ID = {0}. Reason: {1}", delta.Id, ex.Message);
			}
		}

		private void Initialize()
		{
			List<Delta> result = ReadAllDeltas();

			foreach (Delta delta in result)
			{
				try
				{
					foreach (ResourceDescription rd in delta.InsertOperations)
					{
						InsertEntity(rd);
					}

					foreach (ResourceDescription rd in delta.UpdateOperations)
					{
						UpdateEntity(rd);
					}

					foreach (ResourceDescription rd in delta.DeleteOperations)
					{
						DeleteEntity(rd);
					}
				}
				catch(Exception ex)
				{
					CommonTrace.WriteTrace(CommonTrace.TraceError, "Error while applying delta (id = {0}) during service initialization. {1}", delta.Id, ex.Message);
				}

                foreach (KeyValuePair<DMSType, Container> pair in networkDataModelCopy)
                {
                    if(networkDataModel.ContainsKey(pair.Key))
                    {
                        networkDataModel[pair.Key] = pair.Value.Clone() as Container;
                    }
                    else
                    {
                        networkDataModel.Add(pair.Key, pair.Value.Clone() as Container);
                    }
                    
                }
            }		
		}

		private void SaveDelta(Delta delta)
		{
            if (MODE == "DATABASE")
            {
                NmsContext nmsContext = new NmsContext();
                DeltaModel deltaModel = new DeltaModel();
                deltaModel.Time = DateTime.Now;
                deltaModel.Delta = ObjectToByteArray(delta);
                nmsContext.DeltaModels.Add(deltaModel);
                nmsContext.SaveChanges();

                string message = string.Format("Insert new Delta into database successfully finished. ");
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine(message);
            }
            else
            {
                bool fileExisted = false;

                if (File.Exists(Config.Instance.ConnectionString))
                {
                    fileExisted = true;
                }

                FileStream fs = new FileStream(Config.Instance.ConnectionString, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                fs.Seek(0, SeekOrigin.Begin);

                BinaryReader br = null;
                int deltaCount = 0;

                if (fileExisted)
                {
                    br = new BinaryReader(fs);
                    deltaCount = br.ReadInt32();
                }

                BinaryWriter bw = new BinaryWriter(fs);
                fs.Seek(0, SeekOrigin.Begin);

                delta.Id = ++deltaCount;
                byte[] deltaSerialized = delta.Serialize();
                int deltaLength = deltaSerialized.Length;

                bw.Write(deltaCount);
                fs.Seek(0, SeekOrigin.End);
                bw.Write(deltaLength);
                bw.Write(deltaSerialized);

                if (br != null)
                {
                    br.Close();
                    br.Dispose();
                }

                bw.Close();
                bw.Dispose();
                fs.Close();
                fs.Dispose();
            }
        }


        // Convert an object to a byte array
        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
            {
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        // Convert a byte array to an Object
        private Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }

        private List<Delta> ReadAllDeltas()
        {
            if (MODE == "DATABASE")
            {
                string message = string.Empty;
                List<Delta> result = new List<Delta>();
                List<DeltaModel> resultDelta = new List<DeltaModel>();
                NmsContext nmsContext = new NmsContext();
                try
                {
                    resultDelta = nmsContext.DeltaModels.OrderBy(x => x.Time).ToList();

                    foreach (var item in resultDelta)
                    {
                        byte[] delta_byte = item.Delta as byte[];
                        result.Add(ByteArrayToObject(delta_byte) as Delta);

                        TraceDelta(ByteArrayToObject(delta_byte) as Delta);
                    }
                }
                catch (Exception e)
                {
                    message = string.Format("Failed to read Delta from database. {0}", e.Message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    Console.WriteLine(message);
                    return new List<Delta>();
                }

                deltaCount = result.Count;

                message = string.Format("Successfully read {0} Delta from database.", result.Count.ToString());
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
                Console.WriteLine(message);

                return result;

            }
            else
            {
                List<Delta> result = new List<Delta>();

                if (!File.Exists(Config.Instance.ConnectionString))
                {
                    return result;
                }

                FileStream fs = new FileStream(Config.Instance.ConnectionString, FileMode.OpenOrCreate, FileAccess.Read);
                fs.Seek(0, SeekOrigin.Begin);

                if (fs.Position < fs.Length) // if it is not empty stream
                {
                    BinaryReader br = new BinaryReader(fs);

                    int deltaCount = br.ReadInt32();
                    int deltaLength = 0;
                    byte[] deltaSerialized = null;
                    Delta delta = null;

                    for (int i = 0; i < deltaCount; i++)
                    {
                        deltaLength = br.ReadInt32();
                        deltaSerialized = new byte[deltaLength];
                        br.Read(deltaSerialized, 0, deltaLength);
                        delta = Delta.Deserialize(deltaSerialized);
                        result.Add(delta);
                    }

                    br.Close();
                }

                fs.Close();

                return result;
            }


            return null;
        }

		private Dictionary<short, int> GetCounters()
		{
			Dictionary<short, int> typesCounters = new Dictionary<short, int>();

			foreach (DMSType type in Enum.GetValues(typeof(DMSType)))
			{
				typesCounters[(short)type] = 0;

				if (networkDataModel.ContainsKey(type))
				{
					typesCounters[(short)type] = GetContainer(type).Count;
				}
			}

			return typesCounters;
		}

        public UpdateResult Prepare(ref Delta delta)
        {
            transactionCallback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();

            string message = string.Empty;

            networkDataModelCopy.Clear();
            foreach (KeyValuePair<DMSType, Container> pair in networkDataModel)
            {
                networkDataModelCopy.Add(pair.Key, pair.Value.Clone() as Container);
            }
            UpdateResult applyResult = ApplyDelta(delta);

            if (applyResult.Result == ResultType.Succeeded)
            {
                message = "OK";
            }
            else
            {
                message = "ERROR";
            }

            transactionCallback.Response(message);
            return applyResult;
        }

        public bool Commit()
        {
            try
            {
                lock (obj)
                {
                    networkDataModel.Clear();
                    foreach (KeyValuePair<DMSType, Container> pair in networkDataModelCopy)
                    {
                        networkDataModel.Add(pair.Key, pair.Value.Clone() as Container);
                    }

                    networkDataModelCopy.Clear();
                    SaveDelta(deltaToCommit);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool Rollback()
        {
            try
            {
                networkDataModelCopy.Clear();
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Transaction rollback successfully finished!");
                return true;
            }
            catch (Exception e)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceError, "Transaction rollback error. Message: {0}", e.Message);
                return false;
            }
        }

        public IdentifiedObject GetEntityCopy(long globalId)
        {
            if (EntityCopyExists(globalId))
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
                IdentifiedObject io = GetContainerCopy(type).GetEntity(globalId);

                return io;
            }
            else
            {
                string message = string.Format("Entity  (GID = 0x{0:x16}) does not exist.", globalId);
                throw new Exception(message);
            }
        }

        public bool EntityCopyExists(long globalId)
        {
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

            if (ContainerCopyExists(type))
            {
                Container container = GetContainerCopy(type);

                if (container.EntityExists(globalId))
                {
                    return true;
                }
            }

            return false;
        }

        private Container GetContainerCopy(DMSType type)
        {
            if (ContainerCopyExists(type))
            {
                return networkDataModelCopy[type];
            }
            else
            {
                string message = string.Format("ContainerCopy does not exist for type {0}.", type);
                throw new Exception(message);
            }
        }

        private bool ContainerCopyExists(DMSType type)
        {
            if (networkDataModelCopy.ContainsKey(type))
            {
                return true;
            }

            return false;
        }
    }
}
