using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Model
{
    public class WindSpeed
    {
        public string Name { get; set; }
        public float Speed { get;  set; }

        public WindSpeed()
        {
            Name = "CO2 Reduction";
            Speed = (float)21.34;
        }
        public WindSpeed(string n, float s)
        {
            Name = n;
            Speed = s;
        }
        //private int CalcularPorcentagem()
        //{
        //    return 47; //Calculo da porcentagem de consumo
        //}
    }
}
