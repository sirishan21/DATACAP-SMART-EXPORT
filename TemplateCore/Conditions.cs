using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SmartExportTemplates
{
    class Conditions
    {
        public Conditions()
        {
            
        }

        public List<string> EvaluateCondition(XmlNode ConditionNode)
        {
            List<string> output = new List<string>();
            //TODO: handle conditions - below is just to test the skeleton code
            output.Add("TODO: This is the output of the condition : " + ((XmlElement)ConditionNode).Name);
            return output;
        }
    }
}
