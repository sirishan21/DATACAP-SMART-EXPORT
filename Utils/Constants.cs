using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExportTemplates.Utils
{
    public class Constants
    {
        //Templat element names
        public static string SE_NAMESPACE = "https://www.w3.org/2001/XMLSchema";
        public static string SE_DATA_NODE_NAME = "se:data";
        public static string SE_APPEND_TO_FILE = "se:appendToFile";
        public static string SE_OUTPUT_FILE_NAME = "se:filename";
        public const string TEXT_NODE_NAME = "#text";
        public const string SE_TAB_NODE_NAME = "se:tab";
        public const string SE_VALUE_NODE_NAME = "se:value";

        public const string TAB_SPACE = "\t";

        //General
        public static string GE_CURRENT_DCO = "GECurrentDCO";
        public static string GE_DCO = "GEDCO";
        public static string GE_LOG_PREFIX = "GELogPrefix";
        public static string GE_DCO_REF_PATTERN = "GEDcoRefPattern";
        public static string GE_EXPORT_CORE = "GEExportCore";
        public static string GE_BATCH_DIR_PATH = "GEBatchDirPath";
        public static string GE_DEF_OUTPUT_FILE = "SmartExport";
        public static string GE_BATCH_XML_FILE = "GEBatchXMLFile";

        //Object types
        public const int Batch = 0;
        public const int Document = 1;
        public const int Page = 2;
        public const int Field = 3;

    }
}
