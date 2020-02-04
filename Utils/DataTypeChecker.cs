using System;
using SmartExportTemplates.Utils;
//using Datacap.Global;

namespace SmartExportTemplates.Utils
{
    class DataTypeChecker
    {

        public string getType(string str)
        {
            string type = "";

            bool boolValue;
            Int32 intValue;
            Int64 bigintValue;
            double doubleValue;
            DateTime dateValue;

            // Place checks higher in if-else statement to give higher priority to type.

            if (bool.TryParse(str, out boolValue))
                type = Constants.DataTypeString.BOOL;
            else if (Int32.TryParse(str, out intValue))
                type = Constants.DataTypeString.INT32;
            else if (Int64.TryParse(str, out bigintValue))
                type = Constants.DataTypeString.INT64;

            else if (double.TryParse(str, out doubleValue))
                type = Constants.DataTypeString.DOUBLE;
            else if (DateTime.TryParse(str, out dateValue)) //replace with logic to identify date types of various formats
            {
                //Dates d = new Dates();
                //d.IsDate(str);
                // d.FormatDateTime()
                type = Constants.DataTypeString.DATE_TIME;
            }
            else type = Constants.DataTypeString.STRING;
            return type;

        }




    }
}
