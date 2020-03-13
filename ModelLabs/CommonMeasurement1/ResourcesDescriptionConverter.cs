using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using FTN.Services.NetworkModelService.DataModel.Meas;
using FTN.Services.NetworkModelService.DataModel.Wires;

namespace CommonMeasurement1
{
    public static class ResourcesDescriptionConverter
    {
        public static T ConvertTo<T>(ResourceDescription rd) where T : IdentifiedObject
        {
            IdentifiedObject io = CreateEntity(rd.Id);

            foreach (Property property in rd.Properties)
            {
                if (property.Id == ModelCode.IDOBJ_GID)
                {
                    continue;
                }

                if (property.Type == PropertyType.ReferenceVector)
                {
                    continue;
                }

                io.SetProperty(property);
            }
            return (T)io;
        }

        public static IdentifiedObject CreateEntity(long globalId)
        {
            short type = ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

            IdentifiedObject io = null;
            switch ((DMSType)type)
            {
                case DMSType.ANALOG:
                    io = new Analog(globalId);
                    break;

                case DMSType.DISCRETE:
                    io = new Discrete(globalId);
                    break;

                case DMSType.GENERATOR:
                    io = new Generator(globalId);
                    break;

                case DMSType.SUBSTATION:
                    io = new Substation(globalId);
                    break;

                case DMSType.BATTERY_STORAGE:
                    io = new BatteryStorage(globalId);
                    break;

                case DMSType.GEOGRAFICAL_REGION:
                    io = new GeographicalRegion(globalId);
                    break;

                default:
                    string message = String.Format("Failed to create entity because specified type ({0}) is not supported.", type);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    throw new Exception(message);
            }

            return io;
        }
    }
}
