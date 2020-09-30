using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.ViewModel
{
    public class FuelEconomyWindowViewModel : ViewModelBase
    {
        long globalId;
        public FuelEconomyWindowViewModel(long gid)
        {
            globalId = gid;
        }
    }
}
