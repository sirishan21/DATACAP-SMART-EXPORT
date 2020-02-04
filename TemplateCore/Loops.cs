using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SmartExportTemplates.TemplateCore
{
    class Loops
    {
        public Loops()
        {

        }

        public List<string> EvaluateLoop(XmlNode LoopNode)
        {
            List<string> output = new List<string>();
            //TODO: handle loops - below is just to test the skeleton code
           // output.Add("TODO: This is the output of the Loop : " + ((XmlElement)LoopNode).Name);
            return output;
        }
    }
}
