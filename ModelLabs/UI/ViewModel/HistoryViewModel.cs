using CalculationEngineContracts;
using CommonMeas;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel.Wires;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Communication;
using UI.Model;

namespace UI.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {
        #region Fields

        private string item_0;
        private string item_1;
        private string item_2;

        private DateTime startTime;
        private DateTime endTime;
        private PeriodValues selectedPeriod;
        private GraphSample graphSampling;
        private ICommand allGeneratorsCheckedCommand;
        private ICommand allGeneratorsUnheckedCommand;
        private ICommand allEnergyConsumersCheckedCommand;
        private ICommand allEnergyConsumersUnheckedCommand;
        private ICommand selectedPeriodCommand;
        private ICommand visibilityCheckedCommand;
        private ICommand visibilityUncheckedCommand;


        private ICommand showDataCommand;

        private ICommand totalProductionGraphCheckedCommand;
        private ICommand totalProductionGraphUnCheckedCommand;
        private ICommand productionForSelectedCheckedCommand;
        private ICommand productionForSelectedUnCheckedCommand;

        private ICommand totalLoadGraphCheckedCommand;
        private ICommand totalLoadGraphUnCheckedCommand;
        private ICommand loadForSelectedCheckedCommand;
        private ICommand loadForSelectedUnCheckedCommand;

        private bool isExpandedSeparated = false;
        private bool isExpandedTotalProduction = false;

        private UIClientNms uiCli;
        private bool totalProductionForSelectedVisible = false;
        private bool totalProductionGraphVisible = true;

        private bool totalLoadForSelectedVisible = false;
        private bool totalLoadGraphVisible = true;

        private ObservableCollection<long> generatorsFromNms = new ObservableCollection<long>();
        private ObservableCollection<KeyValuePair<long, string>> generatorsFromNmsName = new ObservableCollection<KeyValuePair<long, string>>();

        private ObservableCollection<long> energyConsumersFromNms = new ObservableCollection<long>();

        private ObservableCollection<Tuple<double, DateTime>> totalProduction = new ObservableCollection<Tuple<double, DateTime>>();
        private ObservableCollection<Tuple<double, DateTime>> totalProfit = new ObservableCollection<Tuple<double, DateTime>>();
        private ObservableCollection<Tuple<double, DateTime>> totalCost = new ObservableCollection<Tuple<double, DateTime>>();
        
        private ObservableCollection<Tuple<double, DateTime>> coEmission = new ObservableCollection<Tuple<double, DateTime>>();
        private ObservableCollection<Tuple<double, DateTime>> coReduction = new ObservableCollection<Tuple<double, DateTime>>();




        private ObservableCollection<Tuple<double, DateTime>> graphTotalProduction = new ObservableCollection<Tuple<double, DateTime>>();
        private ObservableCollection<Tuple<double, DateTime>> graphProfit = new ObservableCollection<Tuple<double, DateTime>>();

        private ObservableCollection<Tuple<double, DateTime>> graphCost = new ObservableCollection<Tuple<double, DateTime>>();

        private ObservableCollection<Tuple<double, DateTime>> graphCoEmission = new ObservableCollection<Tuple<double, DateTime>>();
        private ObservableCollection<Tuple<double, DateTime>> graphCoReduction = new ObservableCollection<Tuple<double, DateTime>>();


        private ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>> generatorsContainer = new ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>>();
       
        private Dictionary<long, bool> gidToBoolMap = new Dictionary<long, bool>();

        private int resourcesLeft;
        private List<ModelCode> properties;
        private static List<ResourceDescription> internalGen;
        private ModelResourcesDesc modelResourcesDesc;
        private int numberOfResources = 2;
        private int iteratorId;
        private List<ResourceDescription> retList;


        private UICalculationEngineClient proxy;



        #endregion Fields

        #region Properties
        public ObservableCollection<long> GeneratorsFromNms
        {
            get
            {
                return generatorsFromNms;
            }
            set
            {
                generatorsFromNms = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<KeyValuePair<long, string>> GeneratorsFromNmsName
        {
            get
            {
                return generatorsFromNmsName;
            }
            set
            {
                generatorsFromNmsName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<long> EnergyConsumersFromNms
        {
            get
            {
                return energyConsumersFromNms;
            }
            set
            {
                energyConsumersFromNms = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowDataCommand => showDataCommand ?? (showDataCommand = new RelayCommand(ShowDataCommandExecute));

        public bool IsExpandedSeparated
        {
            get
            {
                return isExpandedSeparated;
            }

            set
            {
                isExpandedSeparated = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>> GeneratorsContainer { get => generatorsContainer; set => generatorsContainer = value; }
       
        public bool IsExpandedTotalProduction
        {
            get
            {
                return isExpandedTotalProduction;
            }

            set
            {
                isExpandedTotalProduction = value;
                OnPropertyChanged();
            }
        }

        public ICommand TotalProductionGraphCheckedCommand => totalProductionGraphCheckedCommand ?? (totalProductionGraphCheckedCommand = new RelayCommand(TotalProductionGraphCheckedExecute));

        public ICommand TotalProductionGraphUnCheckedCommand => totalProductionGraphUnCheckedCommand ?? (totalProductionGraphUnCheckedCommand = new RelayCommand(TotalProductionGraphUnCheckedExecute));
        
        public ICommand ProductionForSelectedCheckedCommand => productionForSelectedCheckedCommand ?? (productionForSelectedCheckedCommand = new RelayCommand(ProductionForSelectedCheckedExecute));

        public ICommand ProductionForSelectedUnCheckedCommand => productionForSelectedUnCheckedCommand ?? (productionForSelectedUnCheckedCommand = new RelayCommand(ProductionForSelectedUnCheckedExecute));


        public ICommand TotalLoadGraphCheckedCommand => totalLoadGraphCheckedCommand ?? (totalLoadGraphCheckedCommand = new RelayCommand(TotalLoadGraphCheckedExecute));

        public ICommand TotalLoadGraphUnCheckedCommand => totalLoadGraphUnCheckedCommand ?? (totalLoadGraphUnCheckedCommand = new RelayCommand(TotalLoadGraphUnCheckedExecute));

        public ICommand LoadForSelectedCheckedCommand => loadForSelectedCheckedCommand ?? (loadForSelectedCheckedCommand = new RelayCommand(LoadForSelectedCheckedExecute));

        public ICommand LoadForSelectedUnCheckedCommand => loadForSelectedUnCheckedCommand ?? (loadForSelectedUnCheckedCommand = new RelayCommand(LoadForSelectedUnCheckedExecute));

        public ICommand ChangePeriodCommand => selectedPeriodCommand ?? (selectedPeriodCommand = new RelayCommand(SelectedPeriodCommandExecute));

        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                graphSampling = GraphSample.None;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        public DateTime EndTime
        {
            get { return endTime; }
            set
            {
                endTime = value;
                graphSampling = GraphSample.None;
                OnPropertyChanged(nameof(EndTime));
            }
        }

        public bool TotalProductionForSelectedVisible
        {
            get
            {
                return totalProductionForSelectedVisible;
            }
            set
            {
                totalProductionForSelectedVisible = value;
                OnPropertyChanged(nameof(TotalProductionForSelectedVisible));
            }
        }

        public bool TotalProductionGraphVisible
        {
            get
            {
                return totalProductionGraphVisible;
            }
            set
            {
                totalProductionGraphVisible = value;
                OnPropertyChanged(nameof(TotalProductionGraphVisible));
            }
        }

        public bool TotalLoadForSelectedVisible
        {
            get
            {
                return totalLoadForSelectedVisible;
            }
            set
            {
                totalLoadForSelectedVisible = value;
                OnPropertyChanged(nameof(TotalLoadForSelectedVisible));
            }
        }

        public bool TotalLoadGraphVisible
        {
            get
            {
                return totalLoadGraphVisible;
            }
            set
            {
                totalLoadGraphVisible = value;
                OnPropertyChanged(nameof(TotalLoadGraphVisible));
            }
        }
        public ObservableCollection<Tuple<double, DateTime>> TotalProduction { get => totalProduction; set => totalProduction = value; }
        public ObservableCollection<Tuple<double, DateTime>> TotalProfit { get => totalProfit; set => totalProfit = value; }
        public ObservableCollection<Tuple<double, DateTime>> TotalCost { get => totalCost; set => totalCost = value; }

        public ObservableCollection<Tuple<double, DateTime>> CoEmission { get => coEmission; set => coEmission = value; }
        public ObservableCollection<Tuple<double, DateTime>> CoReduction { get => coReduction; set => coReduction = value; }



        public ObservableCollection<Tuple<double, DateTime>> GraphTotalProduction { get => graphTotalProduction; set => graphTotalProduction = value; }
        public ObservableCollection<Tuple<double, DateTime>> GraphProfit { get => graphProfit; set => graphProfit = value; }
        public ObservableCollection<Tuple<double, DateTime>> GraphCost { get => graphCost; set => graphCost = value; }

        public ObservableCollection<Tuple<double, DateTime>> GraphCoEmission { get => graphCoEmission; set => graphCoEmission = value; }
        public ObservableCollection<Tuple<double, DateTime>> GraphCoReduction { get => graphCoReduction; set => graphCoReduction = value; }



        public PeriodValues SelectedPeriod { get => selectedPeriod; set => selectedPeriod = value; }
        public GraphSample GraphSampling { get => graphSampling; set => graphSampling = value; }

        public ICommand VisibilityCheckedCommand => visibilityCheckedCommand ?? (visibilityCheckedCommand = new RelayCommand<long>(VisibilityCheckedCommandExecute));

        public ICommand VisibilityUncheckedCommand => visibilityUncheckedCommand ?? (visibilityUncheckedCommand = new RelayCommand<long>(VisibilityUncheckedCommandExecute));


        public ICommand AllGeneratorsCheckedCommand => allGeneratorsCheckedCommand ?? (allGeneratorsCheckedCommand = new RelayCommand(AllGeneratorsCheckedCommandExecute));

        public ICommand AllGeneratorsUncheckedCommand => allGeneratorsUnheckedCommand ?? (allGeneratorsUnheckedCommand = new RelayCommand(AllGeneratorsUnheckedCommandExecute));

        public ICommand AllEnergyConsumersCheckedCommand => allEnergyConsumersCheckedCommand ?? (allEnergyConsumersCheckedCommand = new RelayCommand(AllEnergyConsumersCheckedCommandExecute));

        public ICommand AllEnergyConsumersUncheckedCommand => allEnergyConsumersUnheckedCommand ?? (allEnergyConsumersUnheckedCommand = new RelayCommand(AllEnergyConsumersUnheckedCommandExecute));

        public Dictionary<long, bool> GidToBoolMap
        {
            get
            {
                return gidToBoolMap;
            }

            set
            {
                gidToBoolMap = value;
            }
        }

        public string Item_0 { get => item_0; set { item_0 = value; OnPropertyChanged(); } }
        public string Item_1 { get => item_1; set { item_1 = value; OnPropertyChanged(); } }
        public string Item_2 { get => item_2; set { item_2 = value; OnPropertyChanged(); } }

        #endregion Properties
        public HistoryViewModel()
        {
            Title = "History";
            proxy = new UICalculationEngineClient("CalculationEngineUIEndpoint");

            startTime = DateTime.Now.AddHours(-1);
            endTime = DateTime.Now;
            GraphSampling = GraphSample.HourSample;
            SelectedPeriod = PeriodValues.Last_Hour;
            uiCli = new UIClientNms("UIClientNmsEndpoint");
            IntegrityUpdateForGenerators();
            IntegrityUpdateForEnergyConsumer();
        }
        public void IntegrityUpdateForGenerators()
        {
            internalGen = new List<ResourceDescription>(5);
            modelResourcesDesc = new ModelResourcesDesc();
            retList = new List<ResourceDescription>(5);
            properties = new List<ModelCode>(10);
            ModelCode modelCodeGenerator = ModelCode.GENERATOR;
            iteratorId = 0;
            resourcesLeft = 0;
            numberOfResources = 2;
            string message = string.Empty;

            try
            {

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
                internalGen.AddRange(retList);

                foreach (ResourceDescription rd in internalGen)
                {
                    Generator generator = ResourcesDescriptionConverter.ConvertTo<Generator>(rd);
                    if (GeneratorsFromNms.Contains(rd.Id))
                    {
                        continue;
                    }
                    GeneratorsFromNms.Add(rd.Id);
                    GeneratorsFromNmsName.Add(new KeyValuePair<long, string>(rd.Id, generator.Name));
                    GidToBoolMap.Add(rd.Id, false);
                }
                OnPropertyChanged(nameof(GeneratorsFromNms));

            }
            catch (Exception e)
            {
                message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeGenerator, e.Message);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                //return false;
            }

            retList.Clear();
        }

        public void IntegrityUpdateForEnergyConsumer()
        {
            internalGen = new List<ResourceDescription>(5);
            modelResourcesDesc = new ModelResourcesDesc();
            retList = new List<ResourceDescription>(5);
            properties = new List<ModelCode>(10);
            ModelCode modelCodeGenerator = ModelCode.ENERGY_CONSUMER;
            iteratorId = 0;
            resourcesLeft = 0;
            numberOfResources = 2;
            string message = string.Empty;

            try
            {
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
                internalGen.AddRange(retList);

                foreach (ResourceDescription rd in internalGen)
                {
                    if (EnergyConsumersFromNms.Contains(rd.Id))
                    {
                        continue;
                    }
                    EnergyConsumersFromNms.Add(rd.Id);
                    GidToBoolMap.Add(rd.Id, false);
                }
                OnPropertyChanged(nameof(EnergyConsumersFromNms));

            }
            catch (Exception e)
            {
                message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeGenerator, e.Message);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                //return false;
            }

            retList.Clear();
        }

        #region Commands

        private void VisibilityCheckedCommandExecute(long gid)
        {
            GidToBoolMap[gid] = true;
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void VisibilityUncheckedCommandExecute(long gid)
        {
            GidToBoolMap[gid] = false;
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void ProductionForSelectedCheckedExecute(object obj)
        {
            TotalProductionForSelectedVisible = true;
        }

        private void ProductionForSelectedUnCheckedExecute(object obj)
        {
            TotalProductionForSelectedVisible = false;
        }

        private void TotalProductionGraphCheckedExecute(object obj)
        {
            TotalProductionGraphVisible = true;
        }

        private void TotalProductionGraphUnCheckedExecute(object obj)
        {
            TotalProductionGraphVisible = false;
        }

        private void LoadForSelectedCheckedExecute(object obj)
        {
            TotalLoadForSelectedVisible = true;
        }

        private void LoadForSelectedUnCheckedExecute(object obj)
        {
            TotalLoadForSelectedVisible = false;
        }

        private void TotalLoadGraphCheckedExecute(object obj)
        {
            TotalLoadGraphVisible = true;
        }

        private void TotalLoadGraphUnCheckedExecute(object obj)
        {
            TotalLoadGraphVisible = false;
        }



        private void ShowDataCommandExecute(object obj)
        {

            ObservableCollection<Tuple<double, DateTime>> measurementsFromDb;
            ObservableCollection<Tuple<double, DateTime>> tempData;

            ObservableCollection<Tuple<double, DateTime>> tempDataProfit;
            ObservableCollection<Tuple<double, DateTime>> tempDataCost;

            ObservableCollection<Tuple<double, DateTime>> tempDataEmission;
            ObservableCollection<Tuple<double, DateTime>> tempDataReduction;


            ObservableCollection<Tuple<double, DateTime>> tempContainer = new ObservableCollection<Tuple<double, DateTime>>();
            ObservableCollection<Tuple<double, DateTime>> tempContainer1 = new ObservableCollection<Tuple<double, DateTime>>();


            GraphTotalProduction.Clear();
            GraphProfit.Clear();
            GraphCost.Clear();

            GraphCoEmission.Clear();
            GraphCoReduction.Clear();

            GeneratorsContainer.Clear();

            Item_0 = "";
            Item_1 = "";
            Item_2 = "";


            foreach (KeyValuePair<long, bool> keyPair in GidToBoolMap)
            {
                if (keyPair.Value == true)
                {
                    try
                    {
                        measurementsFromDb = new ObservableCollection<Tuple<double, DateTime>>(proxy.GetHistoryMeasurements(keyPair.Key, startTime, endTime));


                        if (measurementsFromDb == null)
                        {
                            continue;
                        }

                        if (graphSampling != GraphSample.None)
                        {
                            DateTime tempStartTime = startTime.ToUniversalTime();
                            DateTime tempEndTime = IncrementTime(tempStartTime);
                            var endUtc = endTime.ToUniversalTime();

                            double averageProduction = 0;

                            while (tempEndTime <= endUtc)
                            {
                                tempData = new ObservableCollection<Tuple<double, DateTime>>(measurementsFromDb.Where(x => x.Item2 > tempStartTime && x.Item2 < tempEndTime));

                                if (tempData != null && tempData.Count != 0)
                                {
                                    averageProduction = tempData.Average(x => x.Item1) / 1000;
                                }
                                else
                                {
                                    averageProduction = 0;
                                }

                                tempStartTime = IncrementTime(tempStartTime);
                                tempEndTime = IncrementTime(tempEndTime);

                                tempContainer.Add(new Tuple<double, DateTime>(averageProduction, tempStartTime.ToLocalTime()));
                            }
                            GeneratorsContainer.Add(new KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>(keyPair.Key, new ObservableCollection<Tuple<double, DateTime>>(tempContainer)));
                            int count = GeneratorsContainer.Count;
                            if (count == 0)
                            {
                                Item_0 = String.Empty;
                                Item_1 = String.Empty;
                                Item_2 = String.Empty;
                                OnPropertyChanged(nameof(Item_0));
                                OnPropertyChanged(nameof(Item_1));
                                OnPropertyChanged(nameof(Item_2));
                            }
                            else if (count == 1)
                            {
                                Item_0 = GeneratorsFromNmsName.FirstOrDefault(x => x.Key == GeneratorsContainer[0].Key).Value;
                                Item_1 = String.Empty;
                                Item_2 = String.Empty;
                                OnPropertyChanged(nameof(Item_0));
                                OnPropertyChanged(nameof(Item_1));
                                OnPropertyChanged(nameof(Item_2));
                            }
                            else if (count == 2)
                            {
                                Item_0 = GeneratorsFromNmsName.FirstOrDefault(x => x.Key == GeneratorsContainer[0].Key).Value;
                                Item_1 = GeneratorsFromNmsName.FirstOrDefault(x => x.Key == GeneratorsContainer[1].Key).Value;
                                Item_2 = String.Empty;
                                OnPropertyChanged(nameof(Item_0));
                                OnPropertyChanged(nameof(Item_1));
                                OnPropertyChanged(nameof(Item_2));
                            }
                            else if (count == 3)
                            {
                                Item_0 = GeneratorsFromNmsName.FirstOrDefault(x => x.Key == GeneratorsContainer[0].Key).Value;
                                Item_1 = GeneratorsFromNmsName.FirstOrDefault(x => x.Key == GeneratorsContainer[1].Key).Value;
                                Item_2 = GeneratorsFromNmsName.FirstOrDefault(x => x.Key == GeneratorsContainer[2].Key).Value;
                            }
                        }
                        else
                        {
                            GeneratorsContainer.Add(new KeyValuePair<long, ObservableCollection<Tuple<double, DateTime>>>(keyPair.Key, new ObservableCollection<Tuple<double, DateTime>>(measurementsFromDb)));
                        }

                        measurementsFromDb.Clear();
                        measurementsFromDb = null;
                        tempContainer.Clear();
                    }
                    catch (Exception ex)
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceError, "[HistoryViewModel] Error ShowDataCommandExecute {0}", ex.Message);
                    }
                }
            }

            List<Tuple<double, DateTime>> allProductionValues = new List<Tuple<double, DateTime>>();
            List<DateTime> timestamps = new List<DateTime>();

            foreach (var keyPair in GeneratorsContainer)
            {
                allProductionValues.AddRange(keyPair.Value.ToList());
            }

            foreach (Tuple<double, DateTime> tuple in allProductionValues)
            {
                timestamps.Add(tuple.Item2);
            }
            timestamps = timestamps.Distinct().ToList();

            var allProductionData = proxy.GetTotalProduction(StartTime, EndTime);

            TotalProduction = new ObservableCollection<Tuple<double, DateTime>>();
            TotalProfit = new ObservableCollection<Tuple<double, DateTime>>();
            TotalCost = new ObservableCollection<Tuple<double, DateTime>>();
            CoReduction = new ObservableCollection<Tuple<double, DateTime>>();
            CoEmission = new ObservableCollection<Tuple<double, DateTime>>();

            foreach (var item in allProductionData)
            {
                var localDate = item.Item1.ToLocalTime();
                TotalProduction.Add(new Tuple<double, DateTime>(item.Item2, localDate));
                TotalProfit.Add(new Tuple<double, DateTime>(item.Item3, localDate));
                TotalCost.Add(new Tuple<double, DateTime>(item.Item4, localDate));
                CoReduction.Add(new Tuple<double, DateTime>(item.Item6, localDate));
                CoEmission.Add(new Tuple<double, DateTime>(item.Item5, localDate));
            }
            
            GraphTotalProduction = new ObservableCollection<Tuple<double, DateTime>>();
            GraphProfit = new ObservableCollection<Tuple<double, DateTime>>();
            GraphCoReduction = new ObservableCollection<Tuple<double, DateTime>>();
            GraphCoEmission = new ObservableCollection<Tuple<double, DateTime>>();



            if (graphSampling != GraphSample.None)
            {
                DateTime tempStartTime = startTime;
                DateTime tempEndTime = IncrementTime(tempStartTime);

                double averageProduction;
                double averageProfit;
                double averageCost;

                double averageEmission;
                double averageReudtion;

                while (tempEndTime <= endTime)
                {
                    tempData = new ObservableCollection<Tuple<double, DateTime>>(TotalProduction.Where(x => x.Item2 >= tempStartTime && x.Item2 < tempEndTime));
                    tempDataProfit = new ObservableCollection<Tuple<double, DateTime>>(TotalProfit.Where(x => x.Item2 >= tempStartTime && x.Item2 < tempEndTime));
                    tempDataCost = new ObservableCollection<Tuple<double, DateTime>>(TotalCost.Where(x => x.Item2 >= tempStartTime && x.Item2 < tempEndTime));
                    tempDataEmission = new ObservableCollection<Tuple<double, DateTime>>(CoEmission.Where(x => x.Item2 >= tempStartTime && x.Item2 < tempEndTime));
                    tempDataReduction = new ObservableCollection<Tuple<double, DateTime>>(CoReduction.Where(x => x.Item2 >= tempStartTime && x.Item2 < tempEndTime));

                    if (tempData != null && tempData.Count != 0)
                    {
                        averageProduction = tempData.Average(x => x.Item1)/1000;
                    }
                    else
                    {
                        averageProduction = 0;
                    }

                    if (tempDataProfit != null && tempDataProfit.Count != 0)
                    {
                        averageProfit = tempDataProfit.Average(x => x.Item1);
                    }
                    else
                    {
                        averageProfit = 0;
                    }
                    if (tempDataEmission != null && tempDataEmission.Count != 0)
                    {
                        averageEmission = tempDataEmission.Average(x => x.Item1);
                    }
                    else
                    {
                        averageEmission = 0;
                    }
                    if (tempDataReduction != null && tempDataReduction.Count != 0)
                    {
                        averageReudtion = tempDataReduction.Average(x => x.Item1);
                    }
                    else
                    {
                        averageReudtion = 0;
                    }


                    if (tempDataCost != null && tempDataCost.Count != 0)
                    {
                        averageCost = tempDataCost.Average(x => x.Item1);
                    }
                    else
                    {
                        averageCost = 0;
                    }

                    tempStartTime = IncrementTime(tempStartTime);
                    tempEndTime = IncrementTime(tempEndTime);
                    GraphTotalProduction.Add(new Tuple<double, DateTime>(averageProduction, tempStartTime));
                    GraphCost.Add(new Tuple<double, DateTime>(averageCost, tempStartTime));
                    GraphProfit.Add(new Tuple<double, DateTime>(averageProfit,tempStartTime));
                    GraphCoReduction.Add(new Tuple<double, DateTime>(averageReudtion, tempStartTime));
                    GraphCoEmission.Add(new Tuple<double, DateTime>(averageEmission, tempStartTime));

                }
            }
            else
            {
                GraphTotalProduction = TotalProduction;
                GraphProfit = TotalProfit;
                GraphCost = TotalCost;

                GraphCoReduction = CoReduction;
                GraphCoEmission = CoEmission;

            }
            IsExpandedSeparated = true;
            IsExpandedTotalProduction = true;

            OnPropertyChanged(nameof(GraphTotalProduction));
            OnPropertyChanged(nameof(GeneratorsContainer));
            OnPropertyChanged(nameof(GraphProfit));
            OnPropertyChanged(nameof(GraphCost));
            OnPropertyChanged(nameof(GraphCoReduction));
            OnPropertyChanged(nameof(GraphCoEmission));
            OnPropertyChanged(nameof(Item_0));
            OnPropertyChanged(nameof(Item_1));
            OnPropertyChanged(nameof(Item_2));



        }

        private void AllGeneratorsCheckedCommandExecute(object obj)
        {
            // GidToBoolMap = GidToBoolMap.Where(x=>).ToDictionary(p => p.Key, p => false);

            //foreach (var v in GidToBoolMap)
            for (int i = 0; i < GidToBoolMap.Count; i++)
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(GidToBoolMap.Keys.ElementAt(i));
                if (type == DMSType.GENERATOR)
                {
                    GidToBoolMap[GidToBoolMap.Keys.ElementAt(i)] = true;
                }
            }

            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void AllGeneratorsUnheckedCommandExecute(object obj)
        {
            for (int i = 0; i < GidToBoolMap.Count; i++)
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(GidToBoolMap.Keys.ElementAt(i));
                if (type == DMSType.GENERATOR)
                {
                    GidToBoolMap[GidToBoolMap.Keys.ElementAt(i)] = false;
                }
            }
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void AllEnergyConsumersCheckedCommandExecute(object obj)
        {
            foreach(var v in GidToBoolMap)
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(v.Key);
                if(type == DMSType.ENERGY_CONSUMER)
                {
                    GidToBoolMap[v.Key] = true;
                }
            }
            
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void AllEnergyConsumersUnheckedCommandExecute(object obj)
        {
            foreach (var v in GidToBoolMap)
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(v.Key);
                if (type == DMSType.ENERGY_CONSUMER)
                {
                    GidToBoolMap[v.Key] = false;
                }
            }
            OnPropertyChanged(nameof(GidToBoolMap));
        }

        private void SelectedPeriodCommandExecute(object obj)
        {
            UpdatePeriod();
        }

        private void UpdatePeriod()
        {
            switch (SelectedPeriod)
            {
                case PeriodValues.Last_Hour:
                    StartTime = DateTime.Now.AddHours(-1);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.HourSample;
                    break;
                case PeriodValues.Today:
                    StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.TodaySample;
                    break;
                case PeriodValues.Last_Month:
                    StartTime = DateTime.Now.AddMonths(-1);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.LastMonthSample;
                    break;
                case PeriodValues.Last_3_Month:
                    StartTime = DateTime.Now.AddMonths(-3);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.Last4MonthSample;
                    break;
                case PeriodValues.Last_6_Month:
                    StartTime = DateTime.Now.AddYears(-1);
                    EndTime = DateTime.Now;
                    graphSampling = GraphSample.YearSample;
                    break;
                default:
                    break;
            }
        }


        private DateTime IncrementTime(DateTime pointTime)
        {
            switch (graphSampling)
            {
                case GraphSample.HourSample:
                    pointTime = pointTime.AddMinutes(5);
                    return pointTime;
                case GraphSample.TodaySample:
                    pointTime = pointTime.AddHours(1);
                    return pointTime;
                case GraphSample.YearSample:
                    pointTime = pointTime.AddMonths(1);
                    return pointTime;
                case GraphSample.LastMonthSample:
                    pointTime = pointTime.AddDays(1);
                    return pointTime;
                case GraphSample.Last4MonthSample:
                    pointTime = pointTime.AddDays(7);
                    return pointTime;
                default:
                    return pointTime;
            }
        }

        #endregion


    }
}
