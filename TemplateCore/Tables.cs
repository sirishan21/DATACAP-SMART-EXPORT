using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using SmartExportTemplates.DCOUtil;
using TDCOLib;
using System.Diagnostics;

namespace SmartExportTemplates.TemplateCore
{
    class Tables
    {
        private TDCOLib.IDCO CurrentDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_CURRENT_DCO);
        private SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        private DataElement dataElement = new DataElement();
        private DCODataRetriever dCODataRetriever = new DCODataRetriever();
        int rowStart = 0;
        int rowEnd = 0;

        public Tables()
        {
        }

        ///       <summary>
        ///       The method extracts the data present in the table.
        ///       <param name="tableNode">XML node of tables</param>
        ///       </summary>
        public void FetchTable(XmlNode tableNode)
        {
            Stopwatch sw = Stopwatch.StartNew();

            TDCOLib.IDCO DCO = null;
            //case when table is used in loops
            if (Globals.Instance.ContainsKey(Constants.forLoopString.CURRENTITERATIONDCO)
                && Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO) is TDCOLib.IDCO)
            {
                DCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO);
            }
            //case when the xml node is associated at the table/field level
            if (DCO == null)
            {
                DCO = CurrentDCO;
            }
            //case when the xml node is associated at the page level
            if (DCO.ObjectType()== Constants.Page )
            {  
                // when association is at page level its mandatory to specify table name
                if(tableNode.Attributes != null && tableNode.Attributes.Count > 0 &&
                    !string.IsNullOrEmpty(tableNode.Attributes["tablename"].Value)) 
                {
                    if (dCODataRetriever.doesPageContainsTable(DCO, tableNode.Attributes["tablename"].Value))
                        DCO = dCODataRetriever.getTableForPage(DCO, tableNode.Attributes["tablename"].Value);
                    else
                        ExportCore.WriteLog(tableNode.Attributes["tablename"] +" table is not found in page "+ DCO.ID);
                }
                else
                {
                    string message = "Its mandatory to specify the table name when the for-each-rows tag is associated at page level.";
                    ExportCore.WriteLog(message);
                    new SmartExportException(message);
                }
            }
 
            //sets the start and end row numbers
            setTableLimits(tableNode, DCO);
            
            ExportCore.WriteDebugLog("Iterating over the table rows:");

            // iterate over rows
            //print rows
            int i = rowStart;
            do
            {
                TDCOLib.DCO row = DCO.GetChild(i);
                if (row.ObjectType() == Constants.Field)
                {
                    i++;
                    Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, row);
                    XmlNode dataNode = tableNode.ChildNodes.Item(0);
                    {
                        if (dataNode.Name == Constants.NodeTypeString.SE_DATA)
                        {
                            dataElement.setIsTableColumn(true);
                            dataElement.EvaluateData(dataNode);
                        }
                    }
                }
                Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, Constants.EMPTYSTRING);
            } while (i < rowEnd);

            ExportCore.WriteDebugLog(" FetchTable("+tableNode+") completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();
        }

        private void setTableLimits(XmlNode tableNode, IDCO DCO)
        {
            rowStart = 0;
            rowEnd = DCO.NumOfChildren();

            if (tableNode.Attributes != null && tableNode.Attributes.Count > 0)
            {
                if (!string.IsNullOrEmpty(tableNode.Attributes["fromrow"].Value)
                    && Int32.Parse(tableNode.Attributes["fromrow"].Value) - 1 > rowStart)
                    rowStart = Int32.Parse(tableNode.Attributes["fromrow"].Value) - 1;
                if (!string.IsNullOrEmpty(tableNode.Attributes["torow"].Value)
                    && Int32.Parse(tableNode.Attributes["torow"].Value) < rowEnd)
                    rowEnd = Int32.Parse(tableNode.Attributes["torow"].Value);
            }
        }
       
    }

}
