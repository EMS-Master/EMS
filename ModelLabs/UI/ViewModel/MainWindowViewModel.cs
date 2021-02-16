using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.ServiceContracts.ServiceFabricProxy;
using FTN.Services.NetworkModelService.DataModel.Core;
using FTN.Services.NetworkModelService.DataModel.Meas;
using FTN.Services.NetworkModelService.DataModel.Wires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Communication;

namespace UI.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public DockManagerViewModel DockManagerViewModel { get; private set; }
        private Dictionary<long, IdentifiedObject> nmsModelMap;

        public Dictionary<long, IdentifiedObject> NmsModelMap { get { return nmsModelMap; } set { nmsModelMap = value; } }
        public HistoryViewModel HistoryViewModel { get => historyViewModel; set => historyViewModel = value; }
        public CommandViewModel CommandViewModel { get => commandViewModel; set => commandViewModel = value; }
        public AlarmSummaryViewModel AlarmSummaryViewModel { get => alarmSummaryViewModel; set => alarmSummaryViewModel = value; }

        private HistoryViewModel historyViewModel;
        private AlarmSummaryViewModel alarmSummaryViewModel;
        private CommandViewModel commandViewModel;
        private DashboardViewModel dashboardViewModel;
        private UIClientNms uiCli;
        public MainWindowViewModel()
        {
            uiCli = new UIClientNms("UIClientNmsEndpoint");
            InitiateIntegrityUpdate();

            DashboardViewModel = new DashboardViewModel();
            DashboardViewModel.Title = "Dashboard";

            var documents = new List<ViewModelBase>();

            AlarmSummaryViewModel = new AlarmSummaryViewModel() { Title = "Alarm Summary" };

            HistoryViewModel = new HistoryViewModel() { Title = "History" };

            CommandViewModel = new CommandViewModel() { Title = "Command" };

            documents.Add(DashboardViewModel);
            documents.Add(new NMSViewModel(new View.NMSView()) { Title = "NMS" });
            documents.Add(new ImporterViewModel() { Title = "Importer" });

            documents.Add(HistoryViewModel);
            documents.Add(AlarmSummaryViewModel);
            documents.Add(CommandViewModel);
            

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
                ModelCode modelCodeEnergyConsumer = ModelCode.ENERGY_CONSUMER;
                ModelCode modelCodeGeographicalRegion = ModelCode.GEOGRAFICAL_REGION;

                int iteratorId = 0;
                int resourcesLeft = 0;
                int numberOfResources = 2;
                string message = string.Empty;

                List<ResourceDescription> retList = new List<ResourceDescription>(5);
                // NetworkModelGDASfProxy nm = new NetworkModelGDASfProxy();
               
                #region getting Generator
                try
                {
                    // first get all synchronous machines from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCodeGenerator);

                    iteratorId = uiCli.GetExtentValues(modelCodeGenerator, properties);
                    resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = uiCli.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);
                    }
                    uiCli.IteratorClose(iteratorId);

                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeGenerator, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);

                    Console.WriteLine("Trying again...");
                    CommonTrace.WriteTrace(CommonTrace.TraceError, "Trying again...");
                    //uiCli = null;
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

                    iteratorId = uiCli.GetExtentValues(modelCodeSubstation, properties);
                    resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = uiCli.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);
                    }
                    uiCli.IteratorClose(iteratorId);

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

                    iteratorId = uiCli.GetExtentValues(modelCodeDiscrete, properties);
                    resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = uiCli.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);
                    }
                    uiCli.IteratorClose(iteratorId);
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

                    iteratorId = uiCli.GetExtentValues(modelCodeAnalog, properties);
                    resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = uiCli.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);
                    }
                    uiCli.IteratorClose(iteratorId);
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

                #region getting EnergyConsumer
                try
                {
                    // third get all enenrgy consumers from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCodeEnergyConsumer);

                    iteratorId = uiCli.GetExtentValues(modelCodeEnergyConsumer, properties);
                    resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = uiCli.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);
                    }
                    uiCli.IteratorClose(iteratorId);
                }
                catch (Exception e)
                {
                    message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeEnergyConsumer, e.Message);
                    Console.WriteLine(message);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    return false;
                }

                foreach (var resDesc in retList)
                {
                    if (NmsModelMap.ContainsKey(resDesc.Id))
                    {
                        NmsModelMap[resDesc.Id] = ResourcesDescriptionConverter.ConvertTo<EnergyConsumer>(resDesc);
                    }
                    else
                    {
                        NmsModelMap.Add(resDesc.Id, ResourcesDescriptionConverter.ConvertTo<EnergyConsumer>(resDesc));
                    }
                }

                // clear retList
                retList.Clear();
                #endregion

                #region getting GeographicalRegion
                try
                {
                    // third get all enenrgy consumers from NMS
                    properties = modelResourcesDesc.GetAllPropertyIds(modelCodeGeographicalRegion);

                    iteratorId = uiCli.GetExtentValues(modelCodeGeographicalRegion, properties);
                    resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = uiCli.IteratorNext(numberOfResources, iteratorId);
                        retList.AddRange(rds);
                        resourcesLeft = uiCli.IteratorResourcesLeft(iteratorId);
                    }
                    uiCli.IteratorClose(iteratorId);
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

        public DashboardViewModel DashboardViewModel
        {
            get
            {
                return dashboardViewModel;
            }

            set
            {
                dashboardViewModel = value;
                OnPropertyChanged();
            }
        }

    }
}
