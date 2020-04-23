//
// Licensed Materials - Property of IBM
//
// 5725-C15
// © Copyright IBM Corp. 1994, 2019 All Rights Reserved
// US Government Users Restricted Rights - Use, duplication or
// disclosure restricted by GSA ADP Schedule Contract with IBM Corp.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExportTemplates.Core
{
    ///       <summary>
    ///       ContentProcessor is an interface that facilitates processing of a smart export XML template.  
    ///       </summary>  
    interface ContentProcessor
    {
        ///       <summary>
        ///       The method returns a list of valid DCO patterns.
        ///       <param name="dcoDefinitionFile">Fully qualified path of the DCO definition file.</param>
        ///       <return>A list of valid DCO patterns.</return>
        ///       </summary>  
        List<string> createDCOPatternList(string dcoDefinitionFile);

        ///       <summary>
        ///       The  processes the XML template.    
        ///       </summary>
        void processNodes();
    }
}
