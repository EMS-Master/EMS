using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServ.DataBaseModels
{
	public class CommandedGenerator
	{
		[Key]
		public int Id { get; set; }
		public long Gid { get; set; }
		public bool CommandingFlag { get; set; }
	}
}
