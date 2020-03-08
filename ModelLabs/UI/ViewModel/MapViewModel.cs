using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.View;

namespace UI.ViewModel
{
    public class MapViewModel : ViewModelBase
    {
        public MapViewModel(MapView mainWindow)
        {
            Title = "Map";
        }
    }
}
