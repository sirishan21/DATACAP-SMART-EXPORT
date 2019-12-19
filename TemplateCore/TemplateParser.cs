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
        private         SmartExportTemplates.SmartExport ExportCore = new SmartExportTemplates.SmartExport();
        
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
            if (node == null || node.Name == null)
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

    }
}
