using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI
{
   public class Config
    {
        private string resultDirecotry = string.Empty;

        public string ResultDirecotry
        {
            get { return resultDirecotry; }
        }

        private Config()
        {
            resultDirecotry = ConfigurationSettings.AppSettings["ResultDirectory"];

            if (!Directory.Exists(resultDirecotry))
            {
                Directory.CreateDirectory(resultDirecotry);
            }
        }

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
