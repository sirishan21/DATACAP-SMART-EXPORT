using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.TemplateCore;
using SmartExportTemplates.Utils;
using static SmartExportTemplates.SmartExport;

namespace SmartExportTemplates.Core
{
    class ContentProcessorWithDoc : ContentProcessor
    {
        TemplateParser templateParser = null;
        
        //Node Parsers
        DataElement dataElement = new DataElement();
        Conditions conditionEvaluator = new Conditions();
        Loops loopEvaluator = new Loops();
        Tables table = new Tables();
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);

        public ContentProcessorWithDoc(TemplateParser parser)
        {
            this.templateParser = parser;
        }

        ///       <summary>
        ///       The method creates a list of valid DCO references.
        ///      
        public List<string> createDCOPatternList(string dcoDefinitionFile)
        {
            List<string> DCOPatterns = new List<string>();

            XmlDocument batchXML = new XmlDocument();
            batchXML.Load(dcoDefinitionFile);
            XmlElement batchRoot = batchXML.DocumentElement;

            XmlNodeList dcoDocumentNodes = batchRoot.SelectNodes("./D"); //Document nodes
            foreach (XmlNode dcoDocumentNode in dcoDocumentNodes)
            {
                string DocumentID = ((XmlElement)dcoDocumentNode).GetAttribute("type");
                XmlNodeList pageList = dcoDocumentNode.SelectNodes("./P");

                foreach (XmlNode pageNode in pageList)
                {
                    string pageID = ((XmlElement)pageNode).GetAttribute("type");
                    XmlNodeList allPageList = batchRoot.SelectNodes("./P");
                    XmlNode currentPage = pageNode;
                    foreach (XmlNode page in allPageList)
                    {
                        if (pageID == ((XmlElement)page).GetAttribute("type"))
                        {
                            currentPage = page;
                            break;
                        }
                    }
                    XmlNodeList fieldList = currentPage.SelectNodes("./F");
                    foreach (XmlNode fieldNode in fieldList)
                    {
                        string fieldID = ((XmlElement)fieldNode).GetAttribute("type");
                        string dcoPattern = DocumentID + "." + pageID + "." + fieldID;
                        DCOPatterns.Add(dcoPattern);
                    }
                }
            }
            return DCOPatterns;
        }

        public void processNodes()
        {
            // Loop through the template and accumulate the output
            while (templateParser.HasNextNode())
            {
                XmlNode currentNode = templateParser.GetNextNode();
                switch (templateParser.GetNodeType(currentNode))
                {
                    case NodeType.Data:
                        dataElement.EvaluateData(currentNode);
                        break;
                    case NodeType.If:
                        conditionEvaluator.EvaluateCondition(currentNode);
                        break;
                    case NodeType.ForEach:
                        loopEvaluator.EvaluateLoop(currentNode);
                        break;
                    case NodeType.ForEachRows:
                        table.FetchTable(currentNode);
                        break;
                    default:
                        if (currentNode.NodeType == XmlNodeType.Element)
                        {
                            ExportCore.WriteLog("Node type [" + ((XmlElement)currentNode).Name + "] not supported. Will be ignored");
                        }
                        break;
                }
            }
        }

    }
}
