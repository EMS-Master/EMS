using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.AlarmsEventsService
{
   public class Config
    {
        private string connectionString = string.Empty;

        public string ConnectionString
        {
            get { return connectionString; }
        }

        //private Config()
        //{
        //    //connectionString = ConfigurationManager.ConnectionStrings["alarmsEventsconnectionString"].ConnectionString;
        //    //connectionString = ConfigurationManager.ConnectionStrings["historyDbConnectionString"].ConnectionString;
        //    connectionString = ConfigurationManager.ConnectionStrings["SqlServerHistoryDB"].ConnectionString;
        //}

        private static Config instance = null;

        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Config();
                }

                return instance;
            }
        }
    }
}
