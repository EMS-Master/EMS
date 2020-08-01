using FTN.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
    public class ModelCodeToStringConverter : IValueConverter
    {
        public static Dictionary<ModelCode, string> modelCodeStringDictionary = new Dictionary<ModelCode, string>()
        {
            {ModelCode.ANALOG,"Analog"},
            {ModelCode.ANALOG_MAX_VALUE,"Analog_Max_Value"},
            {ModelCode.ANALOG_MIN_VALUE,"Analog_Min_Value"},
            {ModelCode.ANALOG_NORMAL_VALUE,"Analog_Normal_Value"},
            {ModelCode.CONDUCTING_EQUIPMENT,"Conducting_Equipment"},
            {ModelCode.ENERGY_CONSUMER,"Energy_Consumer"},
            {ModelCode.ENERGY_CONSUMER_PFIXED,"Energy_Consumer_Pfixed"},
            {ModelCode.ENERGY_CONSUMER_CURRENT_POWER,"Energy_Consumer_Current_Power"},
            {ModelCode.EQUIPMENT,"Equipment"},
            {ModelCode.EQUIPMENT_CONTAINER,"Equipment_Container"},
            {ModelCode.EQUIPMENT_CONTAINER_EQUIPMENTS,"Equipment_Container_Equipments"},
            {ModelCode.EQUIPMENT_EQUIPMENT_CONTAINER,"Equipment_Equipment_Container"},
            {ModelCode.IDOBJ,"IDobj"},
            {ModelCode.IDOBJ_ALIASNAME,"Aliasname"},
            {ModelCode.IDOBJ_GID,"Gid"},
            {ModelCode.IDOBJ_MRID,"Mrid"},
            {ModelCode.IDOBJ_NAME,"Name"},
            {ModelCode.MEASUREMENT,"Measurement"},
            {ModelCode.MEASUREMENT_TYPE,"MeasurementType"},
            {ModelCode.MEASUREMENT_DIRECTION,"Direction"},
            {ModelCode.MEASUREMENT_SCADA_ADDRESS,"ScadaAddress"},
            {ModelCode.MEASUREMENT_POWER_SYS_RESOURCE,"PowerSysResource"},
            {ModelCode.REGULATING_CONDUCTING_EQUIPMENT,"Regulating_Conducting_Equipment"},
            {ModelCode.ROTATING_MACHINE,"Rotating_Machine"},
            {ModelCode.ROTATING_MACHINE_RATED_S,"RatedS"},
            {ModelCode.GENERATOR,"Generator"},
            {ModelCode.GENERATOR_MAX_Q,"MaxQ"},
            {ModelCode.GENERATOR_MIN_Q,"MinQ"},
            {ModelCode.GENERATOR_TYPE,"GenType"},
            //{ModelCode.BATTERY_STORAGE,"Battery_Storage"},
            //{ModelCode.BATTERY_STORAGE_MAX_POWER,"MaxPower"},
            //{ModelCode.BATTERY_STORAGE_MIN_CAPACITY,"MinCapacity"},
            {ModelCode.DISCRETE,"Discrete"},
            {ModelCode.DISCRETE_MAX_VALUE,"Discrete_MaxValue"},
            {ModelCode.DISCRETE_MIN_VALUE,"Discrete_MinValue"},
            {ModelCode.DISCRETE_NORMAL_VALUE,"Discrete_NormalValue"},
            {ModelCode.CONECTIVITY_NODE_CONTAINER,"ConectivityNodeContainer"},
            {ModelCode.GEOGRAFICAL_REGION,"Geografical_Region"},
            {ModelCode.SUBSTATION,"Substation"},
            {ModelCode.PSR,"Psr"},
            {ModelCode.PSR_MEASUREMENTS,"Psr_Measurements"},


        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && (value is ModelCode))
            {
                return "";
            }

            var modelCode = (ModelCode)value;

            if (modelCodeStringDictionary.ContainsKey(modelCode))
            {
                return modelCodeStringDictionary[modelCode];
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
