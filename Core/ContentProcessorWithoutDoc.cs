using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.DCOUtil;
using SmartExportTemplates.TemplateCore;
using SmartExportTemplates.Utils;
using static SmartExportTemplates.SmartExport;

namespace SmartExportTemplates.Core
{
    ///       <summary>
    ///       ContentProcessorWithoutDoc is an implementation class that facilitates processing of a
    ///       smart export XML template for projects that don't contain Document in their DCO hierarchy.  
    ///       </summary>  
    class ContentProcessorWithoutDoc : ContentProcessor
    {

        TemplateParser templateParser = null;
        
        //Node Parsers
        DataElement dataElement = new DataElement();
        Conditions conditionEvaluator = new Conditions();
        LoopsWithoutDoc loopEvaluator = new LoopsWithoutDoc();
        Tables table = new Tables();
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        TDCOLib.IDCO CurrentDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_CURRENT_DCO);

        public ContentProcessorWithoutDoc(TemplateParser parser)
        {
            this.templateParser = parser;
        }

        public List<string> createDCOPatternList(string dcoDefinitionFile)
        {
            List<string> DCOPatterns = new List<string>();

            XmlDocument batchXML = new XmlDocument();
            batchXML.Load(dcoDefinitionFile);
            XmlElement batchRoot = batchXML.DocumentElement;

            {
                XmlNodeList dcoPageNodes = batchRoot.SelectNodes("./P"); //Page nodes
                foreach (XmlNode dcoPageNode in dcoPageNodes)
                {
                    string PageID = ((XmlElement)dcoPageNode).GetAttribute("type");
                    XmlNodeList fieldList = dcoPageNode.SelectNodes("./F");
                    foreach (XmlNode fieldNode in fieldList)
                    {
                        string fieldID = ((XmlElement)fieldNode).GetAttribute("type");
                        string dcoPattern = PageID + "." + fieldID;
                        DCOPatterns.Add(dcoPattern);
                    }
                }
            }
            return DCOPatterns;
        }

        public void processNodes()
        {
            DCODataRetrieverWithoutDoc dCODataRetriever = new DCODataRetrieverWithoutDoc();
             if (CurrentDCO.ObjectType() == Constants.Batch )
            {
                dCODataRetriever.createFilePageMap(); 
            }
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
