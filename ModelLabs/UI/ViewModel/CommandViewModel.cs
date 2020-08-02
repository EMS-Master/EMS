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
using System.Windows.Input;

namespace UI.ViewModel
{
    public class CommandViewModel : ViewModelBase
    {
        private readonly EmsContext _context = new EmsContext();

        private int resourcesLeft;
        private List<ModelCode> properties;
        private static List<ResourceDescription> internalGen;
        private ModelResourcesDesc modelResourcesDesc;
        private int numberOfResources = 2;
        private int iteratorId;
        private List<ResourceDescription> retList;
        private ObservableCollection<ModelForCheckboxes> gens =  new ObservableCollection<ModelForCheckboxes>();
        private ObservableCollection<ModelForCheckboxes> baterry = new ObservableCollection<ModelForCheckboxes>();
        private ICommand activateGen;
        public ICommand ActivateGen => activateGen ?? (activateGen = new RelayCommand<object>(ActivateGenExecute));

		private ICommand commandGen;
		public ICommand CommandGen => commandGen ?? (commandGen = new RelayCommand<object>(CommandGenExecute));

		private ICommand checkedCommand;
        private ICommand uncheckedCommand;

        public ICommand CheckedCommand => checkedCommand ?? (checkedCommand = new RelayCommand<long>(VisibilityCheckedCommandExecute));

        public ICommand UncheckedCommand => uncheckedCommand ?? (uncheckedCommand = new RelayCommand<long>(VisibilityUncheckedCommandExecute));


        private void VisibilityCheckedCommandExecute(long gid)
        {
            long l;
        }

        private void VisibilityUncheckedCommandExecute(long gid)
        {
            long l;
        }



        private void ActivateGenExecute(object obj)
        {
            ModelForCheckboxes model = (ModelForCheckboxes)obj;
            ScadaCommandingProxy.Instance.CommandDiscreteValues(model.Id, model.IsActive);
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
            IntegrityUpdate();
        }

        public void IntegrityUpdate()
        {
            string message = string.Empty;
            internalGen = new List<ResourceDescription>(5);
            modelResourcesDesc = new ModelResourcesDesc();
            retList = new List<ResourceDescription>(5);
            properties = new List<ModelCode>(10);
            ModelCode modelCodeGenerator = ModelCode.GENERATOR;
            ModelCode modelCodeEnergyConsumer = ModelCode.ENERGY_CONSUMER;
            iteratorId = 0;
            resourcesLeft = 0;
            numberOfResources = 2;

            try
            {
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
                internalGen.AddRange(retList);

                var descrete = _context.DiscreteCounters.ToList();

                foreach (ResourceDescription rd in internalGen)
                {
                    if (Gens.Any(x=> x.Id == rd.Id))
                    {
                        continue;
                    }

                    bool active = descrete.Where(x => x.Gid == rd.Id).FirstOrDefault().CurrentValue;
                    Gens.Add(new ModelForCheckboxes() { Id = rd.Id, IsActive = active });

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

		public float InputValue { get; set; }
    }
}
