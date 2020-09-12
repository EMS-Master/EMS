using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Model
{
    public class ColumChartData
    {
        public string Type { get; set; }
        public float Production { get; set; }

        public ColumChartData()
        {
        }
        public ColumChartData(string n, float s)
        {
            Type = n;
            Production = s;
        }
    }
}
