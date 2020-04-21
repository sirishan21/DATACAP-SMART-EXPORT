using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExportTemplates.Core
{
    interface ContentProcessor
    {
         List<string> createDCOPatternList(string dcoDefinitionFile);
         void processNodes();
    }
}
