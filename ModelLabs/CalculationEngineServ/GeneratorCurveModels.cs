﻿using CommonMeas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ
{
    [Serializable]
	public class GeneratorCurveModels
	{
		private List<GeneratorCurveModel> curves = new List<GeneratorCurveModel>(20);

		public List<GeneratorCurveModel> Curves
		{
			get { return curves; }
			set { curves = value; }
		}

		public GeneratorCurveModels()
		{

		}
	}
}
