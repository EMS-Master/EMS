using CalculationEngineServ;
using FTN.Common;
using FTN.ServiceContracts;
using ScadaContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace UI.ViewModel
{
    public class CommandViewModel : ViewModelBase
    {
        private readonly EmsContext _context = new EmsContext();

        private int resourcesLeft;
        private int resourcesLeft2;


        private List<ModelCode> properties;
        private List<ModelCode> properties2;


        private static List<ResourceDescription> internalGen;
        private ModelResourcesDesc modelResourcesDesc;
        private ModelResourcesDesc modelResourcesDesc2;


        private int numberOfResources = 2;
        private int iteratorId;
        private int iteratorId2;

        private List<ResourceDescription> retList;
        private List<ResourceDescription> retList2;


        private ObservableCollection<ModelForCheckboxes> gens =  new ObservableCollection<ModelForCheckboxes>();
        private ObservableCollection<ModelForCheckboxes> baterry = new ObservableCollection<ModelForCheckboxes>();
        private ICommand activateGen;
        public ICommand ActivateGen => activateGen ?? (activateGen = new RelayCommand<object>(ActivateGenExecute));

		private ICommand commandGenMessBox;
		public ICommand CommandGenMessBox => commandGenMessBox ?? (commandGenMessBox = new RelayCommand<object>(CommandGenMessBoxExecute));

        private TestGDA testGda;

        private void ActivateGenExecute(object obj)
        {
            ModelForCheckboxes model = (ModelForCheckboxes)obj;
            ScadaCommandingProxy.Instance.CommandDiscreteValues(model.Id, model.IsActive);
        }
        private void CommandGenMessBoxExecute(object obj)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to command this element?", "Command", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                CommandGenExecute(obj);
            }
        }

   
        private void CommandGenExecute(object obj)
		{
			ModelForCheckboxes model = (ModelForCheckboxes)obj;
			if(model.IsActive)
			{
				ScadaCommandingProxy.Instance.CommandAnalogValues(model.Id, model.InputValue);
			}
		}


		public ObservableCollection<ModelForCheckboxes> Gens
        {
            get
            {
                return gens;
            }
            set
            {
                gens = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ModelForCheckboxes> EnergyConsumer
        {
            get
            {
                return baterry;
            }
            set
            {
                baterry = value;
                OnPropertyChanged();
            }
        }

        public CommandViewModel()
        {
            Title = "Command";
            testGda = new TestGDA();

            IntegrityUpdate();
        }

        public void IntegrityUpdate()
        {
            string message = string.Empty;
            internalGen = new List<ResourceDescription>(5);
            modelResourcesDesc = new ModelResourcesDesc();
            modelResourcesDesc2 = new ModelResourcesDesc();


            retList = new List<ResourceDescription>(5);
            retList2 = new List<ResourceDescription>(5);


            properties = new List<ModelCode>(10);
            properties2 = new List<ModelCode>(10);


            ModelCode modelCodeGenerator = ModelCode.GENERATOR;
            ModelCode modelCodeEnergyConsumer = ModelCode.ENERGY_CONSUMER;
            ModelCode modelCodeAnalog= ModelCode.ANALOG;


            iteratorId = 0;
            iteratorId2 = 0;


            resourcesLeft = 0;
            resourcesLeft2 = 0;


            numberOfResources = 2;

            try
            {
                properties = modelResourcesDesc.GetAllPropertyIds(modelCodeGenerator);
                properties2 = modelResourcesDesc.GetAllPropertyIds(modelCodeAnalog);


                iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeGenerator, properties);
                iteratorId2 = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeAnalog, properties2);




                resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                resourcesLeft2 = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId2);


                //var retExtentValues = testGda.GetExtentValues(ModelCode.ANALOG, properties.ToList());

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                }

                //while (resourcesLeft2 > 0)
                //{
                //    List<ResourceDescription> rds2 = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId2);
                //    retList2.AddRange(rds2);
                //    foreach (var item in rds2)
                //    {
                //        if (retList.Where(x => x.Id == item.Id).FirstOrDefault() != null)
                //            retList.Where(x => x.Id == item.Id).FirstOrDefault().Properties[6] = item.Properties[6];
                //    }
                //    resourcesLeft2 = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId2);
                //}

                NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);
                internalGen.AddRange(retList);

                var descrete = _context.DiscreteCounters.ToList();

                foreach (ResourceDescription rd in internalGen)
                {
                    if (Gens.Any(x=> x.Id == rd.Id))
                    {
                        continue;
                    }

                    bool active = descrete.Where(x => x.Gid == rd.Id).FirstOrDefault().CurrentValue;
                    float inputValue = _context.HistoryMeasurements.Where(x => x.Gid == rd.Id).OrderByDescending(x => x.MeasurementTime).First().MeasurementValue;
                    Gens.Add(new ModelForCheckboxes() { Id = rd.Id, IsActive = active, InputValue = inputValue, Name = rd.Properties[6].ToString(), Element = "Generator", Gid=rd.Id});

                }
                OnPropertyChanged(nameof(Gens));


                properties = modelResourcesDesc.GetAllPropertyIds(modelCodeEnergyConsumer);
                iteratorId = NetworkModelGDAProxy.Instance.GetExtentValues(modelCodeEnergyConsumer, properties);
                resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = NetworkModelGDAProxy.Instance.IteratorNext(numberOfResources, iteratorId);
                    retList.AddRange(rds);
                    resourcesLeft = NetworkModelGDAProxy.Instance.IteratorResourcesLeft(iteratorId);
                }
                NetworkModelGDAProxy.Instance.IteratorClose(iteratorId);
                internalGen.AddRange(retList);

               
                foreach (ResourceDescription rd in internalGen)
                {
                    if (EnergyConsumer.Any(x => x.Id == rd.Id))
                    {
                        continue;
                    }

                    bool active = descrete.Where(x => x.Gid == rd.Id).FirstOrDefault().CurrentValue;
                    EnergyConsumer.Add(new ModelForCheckboxes() { Id = rd.Id, IsActive = active });

                }
                OnPropertyChanged(nameof(EnergyConsumer));

            }
            catch (Exception e)
            {

                message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCodeGenerator, e.Message);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }
        }

    }

    public class ModelForCheckboxes
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public string Element { get; set; }
        public long Gid { get; set; }
        public string Name { get; set; }
		public float InputValue { get; set; }
    }
}
