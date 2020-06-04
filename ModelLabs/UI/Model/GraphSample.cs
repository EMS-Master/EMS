using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Model
{
    public enum GraphSample : int
    {
        None = 0,
        MinuteSample = 1,
        HourSample = 2,
        TodaySample = 3,
        LastMonthSample = 4,
        Last4MonthSample = 5,
        YearSample = 6
    }
}
