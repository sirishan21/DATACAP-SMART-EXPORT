using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExportTemplates.Utils
{
    public class Constants
    {
        // Logical operators - TODO - there should be a better way to do this!
        public struct CondLogOperators
        {
            internal const string COND_LOG_AND = "and";
            internal const string COND_LOG_OR = "or";
        }

        //Node types
        public struct NodeTypeString
        {
            internal const string SE_DATA = "se:data";
            internal const string SE_IF = "se:if";
            internal const string SE_ELSIF = "se:elsif";
            internal const string SE_ELSE = "se:else";
            internal const string SE_FOREACH = "se:for-each";
        }

        //Templat element names
        public static string SE_NAMESPACE_URL = "https://www.w3.org/2001/XMLSchema";
        public static string SE_NAMESPACE_NAME = "se";
        public static string SE_DATA_NODE_NAME = "se:data";
        public static string SE_APPEND_TO_FILE = "se:appendToFile";
        public static string SE_OUTPUT_FILE_NAME = "se:filename";
        public static string SE_OUTPUT_DIR_PATH = "se:outputFolder";
        public static string SE_ATTRIBUTE_COND_TEST = "test";


        //General
        public static string GE_TEMPLATE_PARSER = "GETemplateParser";
        public static string GE_CURRENT_DCO = "GECurrentDCO";
        public static string GE_DCO = "GEDCO";
        public static string GE_LOG_PREFIX = "GELogPrefix";
        public static string GE_DCO_REF_PATTERN = "GEDcoRefPattern";
        public static string GE_EXPORT_CORE = "GEExportCore";
        public static string GE_BATCH_DIR_PATH = "GEBatchDirPath";
        public static string GE_DEF_OUTPUT_FILE = "SmartExport";
        public static string LOG_PREFIX = "DBA-SmartExport - ";
        public static string DCO_REF_PATTERN = "\\[DCO\\..+?\\..+?\\..+?\\]";
        public static string IF_REF_PATTERN = "(and|or)";


    }
}