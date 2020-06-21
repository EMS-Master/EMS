using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel.Core;
using FTN.Services.NetworkModelService.DataModel.Meas;
using FTN.Services.NetworkModelService.DataModel.Wires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UI.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public DockManagerViewModel DockManagerViewModel { get; private set; }
        public Dictionary<long, IdentifiedObject> NmsModelMap { get; set; }
        public HistoryViewModel HistoryViewModel { get => historyViewModel; set => historyViewModel = value; }
        public AlarmSummaryViewModel AlarmSummaryViewModel { get => alarmSummaryViewModel; set => alarmSummaryViewModel = value; }

        private HistoryViewModel historyViewModel;
        private AlarmSummaryViewModel alarmSummaryViewModel;

        public MainWindowViewModel()
        {
            InitiateIntegrityUpdate();

            var documents = new List<ViewModelBase>();

            AlarmSummaryViewModel = new AlarmSummaryViewModel();
            AlarmSummaryViewModel.Title = "Alarm Summary";

            HistoryViewModel = new HistoryViewModel() { Title = "History" };


            documents.Add(new NMSViewModel(new View.NMSView()) { Title = "NMS" });
            documents.Add(new ImporterViewModel() { Title = "Importer" });
            documents.Add(new MapViewModel(new View.MapView()) { Title = "Map" });
            documents.Add(HistoryViewModel);
            documents.Add(AlarmSummaryViewModel);


            this.DockManagerViewModel = new DockManagerViewModel(documents);
        }

        public bool InitiateIntegrityUpdate()
        {
            //lock (lockObj)
            {
                Thread.Sleep(5000); //just for testing, remove
                NmsModelMap = new Dictionary<long, IdentifiedObject>();
                ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();

                List<ModelCode> properties = new List<ModelCode>(10);
                ModelCode modelCodeDiscrete = ModelCode.DISCRETE;
                ModelCode modelCodeAnalog = ModelCode.ANALOG;
                ModelCode modelCodeGenerator = ModelCode.GENERATOR;
                ModelCode modelCodeSubstation= ModelCode.SUBSTATION;
                ModelCode modelCodeBatteryStorage = ModelCode.BATTERY_STORAGE;
                ModelCode modelCodeGeographicalRegion = ModelCode.GEOGRAFICAL_REGION;

                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 2;
                string message = string.Empty;

                List<ResourceDescription> retList = new List<ResourceDescription>(5);

                #region getting Generator
                try
                {
                    // first get all synchronous machines from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCodeGenerator);

                    iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeGenerator, properties);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                    }
                    NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);

                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeGenerator, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);

                    Console.WriteLine("Trying again...");
                    CommonTrace.WriteTrace(CommonTrace.TraceError, "Trying again...");
                    NetworkModelGDAProxy.Instance = null;
                    Thread.Sleep(1000);
                    InitiateIntegrityUpdate();
                }

                foreach (var resDesc in retList)
                {
                    if (NmsModelMap.ContainsKey(resDesc.Id))
                    {
                        NmsModelMap[resDesc.Id] = ResourcesDescriptionConverter.ConvertTo<Generator>(resDesc);
                    }
                    else
                    {
                        NmsModelMap.Add(resDesc.Id, ResourcesDescriptionConverter.ConvertTo<Generator>(resDesc));
                    }
                }

                // clear retList for getting new model from NMS
                retList.Clear();

                properties.Clear();
                iteratorId = 0;
                resourcesLeft = 0;
                #endregion
                #region getting Substation
                try
                {
                    // second get all ems fuels from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCodeSubstation);

                    iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeSubstation, properties);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                    }
                    NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);

                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeSubstation, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    return false;
                }

                foreach (var resDesc in retList)
                {
                    if (NmsModelMap.ContainsKey(resDesc.Id))
                    {
                        NmsModelMap[resDesc.Id] = ResourcesDescriptionConverter.ConvertTo<Substation>(resDesc);
                    }
                    else
                    {
                        NmsModelMap.Add(resDesc.Id, ResourcesDescriptionConverter.ConvertTo<Substation>(resDesc));
                    }
                }

                // clear retList for getting new model from NMS
                retList.Clear();
                #endregion
                #region getting Discrete
                try
                {
                    // third get all enenrgy consumers from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCodeDiscrete);

                    iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeDiscrete, properties);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                    }
                    NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);
                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeDiscrete, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    return false;
                }

                foreach (var resDesc in retList)
                {
                    if (NmsModelMap.ContainsKey(resDesc.Id))
                    {
                        NmsModelMap[resDesc.Id] = ResourcesDescriptionConverter.ConvertTo<Discrete>(resDesc);
                    }
                    else
                    {
                        NmsModelMap.Add(resDesc.Id, ResourcesDescriptionConverter.ConvertTo<Discrete>(resDesc));
                    }
                }

                // clear retList
                retList.Clear();
                #endregion

                #region getting Analog
                try
                {
                    // third get all enenrgy consumers from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCodeAnalog);

                    iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeAnalog, properties);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                    }
                    NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);
                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeAnalog, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    return false;
                }

                foreach (var resDesc in retList)
                {
                    if (NmsModelMap.ContainsKey(resDesc.Id))
                    {
                        NmsModelMap[resDesc.Id] = ResourcesDescriptionConverter.ConvertTo<Analog>(resDesc);
                    }
                    else
                    {
                        NmsModelMap.Add(resDesc.Id, ResourcesDescriptionConverter.ConvertTo<Analog>(resDesc));
                    }
                }

                // clear retList
                retList.Clear();
                #endregion

                #region getting BattaryStorage
                try
                {
                    // third get all enenrgy consumers from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCodeBatteryStorage);

                    iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeBatteryStorage, properties);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                    }
                    NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);
                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeBatteryStorage, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    return false;
                }

                //foreach (var resDesc in retList)
                //{
                //    if (NmsModelMap.ContainsKey(resDesc.Id))
                //    {
                //        NmsModelMap[resDesc.Id] = ResourcesDescriptionConverter.ConvertTo<BatteryStorage>(resDesc);
                //    }
                //    else
                //    {
                //        NmsModelMap.Add(resDesc.Id, ResourcesDescriptionConverter.ConvertTo<BatteryStorage>(resDesc));
                //    }
                //}

                // clear retList
                retList.Clear();
                #endregion

                #region getting GeographicalRegion
                try
                {
                    // third get all enenrgy consumers from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCodeGeographicalRegion);

                    iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeGeographicalRegion, properties);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                    }
                    NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);
                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeGeographicalRegion, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    return false;
                }

                foreach (var resDesc in retList)
                {
                    if (NmsModelMap.ContainsKey(resDesc.Id))
                    {
                        NmsModelMap[resDesc.Id] = ResourcesDescriptionConverter.ConvertTo<GeographicalRegion>(resDesc);
                    }
                    else
                    {
                        NmsModelMap.Add(resDesc.Id, ResourcesDescriptionConverter.ConvertTo<GeographicalRegion>(resDesc));
                    }
                }

                // clear retList
                retList.Clear();
                #endregion


                OnPropertyChanged(nameof(NmsModelMap));
                return true;
            }
        }

    }
}
