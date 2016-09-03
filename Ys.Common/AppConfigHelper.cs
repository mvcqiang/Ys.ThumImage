using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Ys.Common
{
   public class AppConfigHelper
    {
       public static T GetAppConfig<T>(string key)
       {
          return (T)Convert.ChangeType(ConfigurationManager.AppSettings[key], typeof(T));
       }
    }
}
