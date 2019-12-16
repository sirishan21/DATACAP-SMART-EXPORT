using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SmartExportTemplates.TemplateCore
{
    class TemplateParser
    {
        public TemplateParser(string TemplateFilePath)
        {

        }

        public bool Parse()
        {
            return true;
        }

        public bool HasNextNode()
        {
            return true;
        }

        public XmlNode GetNextNode()
        {
            return null;
        }

    }
}
