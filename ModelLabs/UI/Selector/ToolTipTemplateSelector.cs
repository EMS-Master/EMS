using FTN.Services.NetworkModelService.DataModel.Wires;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UI.Selector
{
    public class ToolTipTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GeneratorTemplate { get; set; }
        public DataTemplate EnergyConsumerTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
                   DependencyObject container)
        {
            if (item is Generator)
            {
                return GeneratorTemplate;
            }
            else if (item is EnergyConsumer)
            {
                return EnergyConsumerTemplate;
            }

            return null;
        }
    }
}
