using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;

namespace SmartExportTemplates.TemplateCore
{
    class DataElement
    {
        public DataElement()
        {

        }

        public List<string> EvaluateData(XmlNode DataNode)
        {
            List<string> output = new List<string>();
            string NodeName = ((XmlElement)DataNode).Name;
            if (!NodeName.Trim().Equals(Constants.SE_DATA_NODE_NAME))
            {
                throw new SmartExportException("Internal error. Data node expected for evaluation but found " + NodeName); 
            }
            output.Add(DataNode.InnerText);
            //TODO: Handle "se:value select" for DCO references - above is just for testing the skeleton
            return output;
        }
    }
}
