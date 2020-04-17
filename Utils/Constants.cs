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
            internal const string SE_ROWS = "se:for-each-rows";
        }

        public struct ConditionString
        {
            internal const string DOCUMENT_TYPE = "document.type";
            internal const string PAGE_TYPE = "page.type";
            internal const string TABLE_TYPE = "table.type";
            internal const string FILE_PAGE_TYPES = "file.page.types";

        }

        public struct forLoopString
        {
            internal const string BATCH = "BATCH";
            internal const string DOCUMENT = "DOCUMENT";
            internal const string PAGE = "PAGE";
            internal const string FIELD = "FIELD";
            internal const string CURRENTITERATIONDCO = "currentIterationDCO";
            internal const string TABLE = "TABLE";
            internal const string CURRENTFILE = "currentFile";
        }

        //Templat element names
        public static string SE_NAMESPACE_URL = "https://www.w3.org/2001/XMLSchema";
        public static string SE_NAMESPACE_NAME = "se";
        public static string SE_DATA_NODE_NAME = "se:data";
        public static string SE_APPEND_TO_FILE = "se:appendToFile";
        public static string SE_OUTPUT_FILE_NAME = "se:filename";
        public static string SE_OUTPUT_FILE_EXTENSION = "se:fileext";
        public static string SE_LOCALE = "se:locale";
        public static string SE_OUTPUT_DIR_PATH = "se:outputFolder";
        public static string SE_ATTRIBUTE_COND_TEST = "test";
        public static string SE_OUTPUT_MEM_CACHE_LINES = "se:memCacheLines";

        //General
        public static string GE_TEMPLATE_PARSER = "GETemplateParser";
        public static string GE_CURRENT_DCO = "GECurrentDCO";
        public const string GE_SMART_NAV = "GESmartNav";
        public static string GE_DCO = "GEDCO";
        public static string GE_ALL_LOG_PREFIX = "ALL: SmartExport :";
        public static string GE_DEBUG_LOG_PREFIX = "DEBUG: SmartExport :";
        public static string GE_INFO_LOG_PREFIX = "INFO: SmartExport :";
        public static string GE_ERROR_LOG_PREFIX = "ERROR: SmartExport :";
        public static string GE_DCO_REF_PATTERN = "GEDcoRefPattern";
        public static string GE_EXPORT_CORE = "GEExportCore";
        public static string GE_BATCH_DIR_PATH = "GEBatchDirPath";
        public static string GE_DEF_OUTPUT_FILE = "SmartExport";
        public static string GE_DEF_OUTPUT_FILE_EXT = "txt";
        public static string GE_TEMP_FILE_PREFIX = "Temp";
        public static string GE_TEMP_FILE_MAP = "tempFileMap";
        public const string GE_DEFAULT_OUTPUT_MEMORY_CACHE_LINES = "3000";
        public static string DCO_REF_PATTERN = "\\[DCO\\]\\.\\[.+?\\]\\.\\[.+?\\]\\.\\[.+?\\]";
        public static string DCO_REF_PATTERN_NO_DOC = "\\[DCO\\]\\.\\[.+?\\]\\.\\[.+?\\]";
        public static string IF_REF_PATTERN = "(and|or)";
        public static string ALLOWED_OPERATORS = "(EQUALS|LESSER-THAN|GREATER-THAN|CONTAINS)";
        public static string LOCALE = "locale";
        internal const string EMPTYSTRING = "";
        internal const string COMMA = ",";
        public static string FILE_PAGE_MAP = "FilePageMap";
        public static string PROJECT_HAS_DOC="projectHasDoc";

        //Object types
        public const int Batch = 0;
        public const int Document = 1;
        public const int Page = 2;
        public const int Field = 3;
        public const string FILE_NAME = "FileName";
        

        public const string TEXT_NODE_NAME = "#text";
        public const string SE_TAB_NODE_NAME = "se:tab";
        public const string SE_COMMA_NODE_NAME = "se:comma";
        public const string SE_VALUE_NODE_NAME = "se:value";
        public const string SE_IMAGE_NAME = "se:imagename";
        public const string SE_FILE_NAME = "se:filename";
        public const string SE_SMART_PARAM_NODE_NAME = "se:smartParam";
        public const string TAB_SPACE = "\t";
        public const string SMARTP_AT = "@";
        public const string SE_NEW_LINE_NODE_NAME = "se:newLine";
        public const string NEW_LINE = "\n";

        public const string DATE_FORMAT = "dd/MMM/yyyy";


        public struct DataTypeString
        {
            internal const string INT = "int";
            internal const string DOUBLE = "double";
            internal const string DATE_TIME = "DateTime";
            internal const string STRING = "string";
            internal const string BOOL = "bool";           
        }

        public struct Operators
        {
            internal const string EQUALS = "EQUALS";
            internal const string LESSER_THAN = "LESSER-THAN";
            internal const string GREATER_THAN = "GREATER-THAN";
            internal const string CONTAINS = "CONTAINS";
        }

    }
}