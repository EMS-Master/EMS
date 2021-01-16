using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CalculationEngineServ
{
	public class LoadXMLFile
	{
		public static GeneratorCurveModels Load()
		{
			string message = string.Empty;
			XmlSerializer serializer = new XmlSerializer(typeof(GeneratorCurveModels));
			StreamReader reader = new StreamReader(@"C:\Users\barba\Desktop\EMS\ModelLabs\Resources\GeneratorsCurves.xml");
			var value = serializer.Deserialize(reader);

			GeneratorCurveModels curveModel = new GeneratorCurveModels();

			try
			{
				curveModel = value as GeneratorCurveModels;
				message = string.Format("Successfully read xml file.");
				Console.WriteLine(message);
			}
			catch (Exception e)
			{
				message = string.Format("Error occured during reading xml file: {0}", e.Message);
				Console.WriteLine(message);
			}

			return curveModel;
		}
	}
}
