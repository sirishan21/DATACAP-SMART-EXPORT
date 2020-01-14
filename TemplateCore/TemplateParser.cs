using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;

namespace SmartExportTemplates.TemplateCore
{
    class TemplateParser
    {
        private string  TemplateFilePath = null;
        XmlElement      TemplateRoot = null;
        XmlNode         CurrentNode = null;
        bool            HasMoreNodes = false;
        XmlNamespaceManager NameSpcManager = null;
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        
        struct NodeTypeString
        {
            internal const string SE_DATA = "se:data";
            internal const string SE_IF = "se:if";
            internal const string SE_FOREACH = "se:for-each";
        }

        public TemplateParser(string TemplateFilePath)
        {
            this.TemplateFilePath = TemplateFilePath;
        }

        public bool Parse() 
        {
            try
            {
                XmlDocument templateXML = new XmlDocument();
                templateXML.Load(TemplateFilePath);
                this.NameSpcManager = new XmlNamespaceManager(templateXML.NameTable);
                this.NameSpcManager.AddNamespace("se", Constants.SE_NAMESPACE);
                this.TemplateRoot = templateXML.DocumentElement;
                if (this.TemplateRoot.HasChildNodes)
                {
                    this.HasMoreNodes = true;
                    this.CurrentNode = TemplateRoot.FirstChild;
                }
            } catch (Exception exp)
            {
                ExportCore.WriteLog(Globals.Instance.GetData("LogPrefix") + "Error while parsing the template file, terminating. Details: " + exp.Message);
                throw new SmartExportException("Error while parsing template file. Please verify the syntax and semantics of the template file.");
            }
            return true;
        }

        public bool HasNextNode()
        {
            return this.HasMoreNodes;
        }

        public XmlNode GetNextNode()
        {
            XmlNode TmpNode = CurrentNode;
            HasMoreNodes = false;
            if (CurrentNode != null && CurrentNode.NextSibling != null)
            {
                CurrentNode = CurrentNode.NextSibling;
                HasMoreNodes = true;
            }

            return TmpNode;
        }

        public int GetNodeType(XmlNode node)
        {
            if (node == null || node.Name == null || node.NodeType != XmlNodeType.Element)
                return SmartExport.NodeType.Invalid;

            int nodeType = SmartExport.NodeType.Invalid;

            switch (node.Name.Trim())
            {
                case NodeTypeString.SE_IF:
                    nodeType = SmartExport.NodeType.If;
                    break;
                case NodeTypeString.SE_FOREACH:
                    nodeType = SmartExport.NodeType.ForEach;
                    break;
                case NodeTypeString.SE_DATA:
                    nodeType = SmartExport.NodeType.Data;
                    break;
            }
            return nodeType;
        }

        public bool AppendToFile()
        {
            bool AppendToFile = false;
            string path = "./" + Constants.SE_APPEND_TO_FILE;
            XmlNode appendToFileNode = TemplateRoot.SelectSingleNode(path, this.NameSpcManager);
            if (appendToFileNode != null)
            {
                string sAppendToFile = appendToFileNode.InnerText.Trim();
                AppendToFile = (sAppendToFile.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
            }
            return AppendToFile;
        }

        public string GetOutputFileName()
        {
            string OutputFileName = Constants.GE_DEF_OUTPUT_FILE;
            XmlNode outputFileNode = TemplateRoot.SelectSingleNode("./" + Constants.SE_OUTPUT_FILE_NAME, this.NameSpcManager);
            if (outputFileNode != null)
            {
                OutputFileName = (outputFileNode.InnerText != null && !outputFileNode.InnerText.Trim().Equals("")) ?
                                    outputFileNode.InnerText.Trim() : Constants.GE_DEF_OUTPUT_FILE;
            }
            return OutputFileName;
        }

        public string GetOutputDirectory()
        {
            //TODO - Get the output dir from the template file and validate it for its existence
            // if not found, create it and return the path. Temporarily, returning the batch dir path

            return (string)Globals.Instance.GetData(Constants.GE_BATCH_DIR_PATH);
        }

    }
}
