using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExportTemplates.Utils
{
    [Serializable]
    class SmartExportException: Exception 
    {
        public SmartExportException()
        {

        }

        public SmartExportException(string message)
            : base(String.Format("SmartExport exception: {0}", message))
        {

        }
    }
}
