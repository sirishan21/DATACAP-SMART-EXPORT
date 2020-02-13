using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using SmartExportTemplates.DCOUtil;

namespace SmartExportTemplates.TemplateCore
{
    class DataElement
    {
        private DCODataRetriever dCODataRetriever = new DCODataRetriever();

        public DataElement()
        {

        }

        public List<string> EvaluateData(XmlNode DataNode)
        {
            List<string> output = new List<string>();
            string NodeName = ((XmlElement)DataNode).Name;

            if (DataNode.HasChildNodes)
            {
                StringBuilder text = new StringBuilder("");
                foreach (XmlNode node in DataNode.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case Constants.TEXT_NODE_NAME:
                            text.Append(node.Value);
                            break;
                        case Constants.SE_TAB_NODE_NAME:
                            text.Append(Constants.TAB_SPACE);
                            break;
                        case Constants.SE_COMMA_NODE_NAME:
                            text.Append(Constants.COMMA);
                            break;
                        case Constants.SE_VALUE_NODE_NAME:
                            text.Append(dCODataRetriever.getDCOValue(node.Attributes["select"].Value));
                            break;
                        default:
                            throw new SmartExportException("Internal error. " + node.Name + " node is not supported inside data node ");
                    }
                }
                if (text.Length > 0)
                {
                    output.Add(text.ToString());
                }

            }
            return output;
        }
    }
}