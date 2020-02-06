using System;
using SmartExportTemplates.Utils;
using Datacap.Global;
using System.Collections.Generic;
namespace SmartExportTemplates.Utils
{
    class DataTypeChecker
    {

        public  List<string> numericTypes = new List<string>() { Constants.DataTypeString.DOUBLE , Constants.DataTypeString.INT   };
         

        public string getType(string str)
        {
            string type = "";

            bool boolValue;
             Int64 bigintValue;
            double doubleValue;
            Dates date = new Dates();
             
            // Place checks higher in if-else statement to give higher priority to type.

            if (bool.TryParse(str, out boolValue))
                type = Constants.DataTypeString.BOOL;
            else if (Int64.TryParse(str, out bigintValue))
                type = Constants.DataTypeString.INT;
            else if (double.TryParse(str, out doubleValue))
                type = Constants.DataTypeString.DOUBLE;
            else if (date.IsDate(str))              
                type = Constants.DataTypeString.DATE_TIME;
            else type = Constants.DataTypeString.STRING;
            return type;

        }




    }
}
