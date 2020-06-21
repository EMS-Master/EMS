using FTN.Services.NetworkModelService.DataModel.Meas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMeas
{
	public class DiscreteLocation : ICloneable
	{
		public DiscreteLocation()
		{
		}
		
		public Discrete Discrete { get; set; }
		
		public int StartAddress { get; set; }
		
		public int Length { get; set; }
		
		public int LengthInBytes { get; set; }

		public object Clone()
		{
			DiscreteLocation dlocation = new DiscreteLocation();
			dlocation.Discrete = Discrete;
			dlocation.StartAddress = StartAddress;
			dlocation.Length = Length;
			dlocation.LengthInBytes = LengthInBytes;

			return dlocation;
		}
	}
}
