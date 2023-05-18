using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;

namespace QuotationsWidgetProvider
{
    public class Utils
    {


        public static string MainList()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var istr = assembly.GetManifestResourceStream("QuotationsWidgetProvider.Templates.mainList.json");
            System.IO.StreamReader sr = new System.IO.StreamReader(istr);
            return sr.ReadToEnd();
        }
    }
}
