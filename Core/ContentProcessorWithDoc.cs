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
    class ContentProcessorWithDoc
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
