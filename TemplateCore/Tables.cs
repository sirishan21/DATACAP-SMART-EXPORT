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
        
        private bool projectHasDoc= (bool) Globals.Instance.GetData(Constants.PROJECT_HAS_DOC);
        int rowStart = 0;
        int rowEnd = 0;
        TDCOLib.IDCO DCO = null;
        TDCOLib.IDCO table = null;
        List<TDCOLib.IDCO> tableDCOs = new List<TDCOLib.IDCO>();

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
           
            setContext();

            if (DCO.ObjectType() == Constants.Field)
            {
                initializeTableObjectForField(tableNode);
            }
            else if (DCO.ObjectType()== Constants.Page )
            {
                initializeTableObjectForPage( tableNode);
            }
            else if(DCO.ObjectType()==Constants.Document)
            {
                initializeTableObjectForDocument(tableNode);                
            }
            else if (DCO.ObjectType() == Constants.Batch && !projectHasDoc)
            {
                initializeTableObjectForBatch(tableNode);       
            }
            else
            {
                throw new SmartExportException("Unable to fetch table.");
            }

            ExportCore.WriteDebugLog("Iterating over the table rows:");

            // iterate over rows
            //print rows
            if (tableDCOs.Count > 0)
            {
                ExportCore.WriteDebugLog("Iterating over multiple table rows:");

                foreach (IDCO table in tableDCOs)
                {
                    setTableLimits(tableNode, table);
                    processTableRows(table, tableNode);
                }
            }
            else if(null != table )
            {
                ExportCore.WriteDebugLog("Iterating over single table rows:");
                setTableLimits(tableNode, table);
                processTableRows(table, tableNode);
            }
            else
            {
                ExportCore.WriteDebugLog("Table not found");

            }

            ExportCore.WriteDebugLog(" FetchTable("+tableNode+") completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();
        }

        private void initializeTableObjectForBatch(XmlNode tableNode)
        {
            DCODataRetrieverWithoutDoc dCODataRetriever = new DCODataRetrieverWithoutDoc();

            string filename = (string)Globals.Instance.GetData(Constants.forLoopString.CURRENTFILE);
            if (!string.IsNullOrEmpty(filename))
            {
                // when association is at page level its mandatory to specify table name
                if (tableNode.Attributes != null && tableNode.Attributes.Count > 0 &&
                    !string.IsNullOrEmpty(tableNode.Attributes["tablename"].Value))
                {
                    tableDCOs.AddRange( dCODataRetriever.getTablesForFile(filename, tableNode.Attributes["tablename"].Value));
                    if (tableDCOs.Count == 0)
                        ExportCore.WriteLog(tableNode.Attributes["tablename"] + " table is not found in document " + DCO.ID);
                }
                else
                {
                    string message = "Its mandatory to specify the table name when the for-each-rows tag is associated at page level.";
                    ExportCore.WriteLog(message);
                    new SmartExportException(message);
                }
            }
        }

        private void initializeTableObjectForDocument(XmlNode tableNode)
        {
            DCODataRetriever dCODataRetriever = new DCODataRetriever();

            // when association is at page level its mandatory to specify table name
            if (tableNode.Attributes != null && tableNode.Attributes.Count > 0 &&
                !string.IsNullOrEmpty(tableNode.Attributes["tablename"].Value))
            {
                tableDCOs.AddRange(dCODataRetriever.getTablesForDocument(DCO, tableNode.Attributes["tablename"].Value));

                if (tableDCOs.Count == 0)
                    ExportCore.WriteLog(tableNode.Attributes["tablename"] + " table is not found in document " + DCO.ID);
            }
            else
            {
                string message = "Its mandatory to specify the table name when the for-each-rows tag is associated at page level.";
                ExportCore.WriteLog(message);
                new SmartExportException(message);
            }
        }

        private void initializeTableObjectForPage(XmlNode tableNode)
        {
             DCODataRetriever dCODataRetriever = new DCODataRetriever();
            // when association is at page level its mandatory to specify table name
            if (tableNode.Attributes != null && tableNode.Attributes.Count > 0 &&
                !string.IsNullOrEmpty(tableNode.Attributes["tablename"].Value))
            {
                table = dCODataRetriever.getTableForPage(DCO, tableNode.Attributes["tablename"].Value);
                if(null== table)
                    ExportCore.WriteLog(tableNode.Attributes["tablename"] + " table is not found in page " + DCO.ID);
            }
            else
            {
                string message = "Its mandatory to specify the table name when the for-each-rows tag is associated at page level.";
                ExportCore.WriteLog(message);
                new SmartExportException(message);
            }
        }

        private void initializeTableObjectForField(XmlNode tableNode)
        {
            DCODataRetriever dCODataRetriever = new DCODataRetriever();
            
            if (dCODataRetriever.isObjectTable(DCO) )
                table = DCO;
           if (null == table)
                ExportCore.WriteLog(" Table is not found in field " + DCO.ID);
           
        }

        private void setContext()
        {
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
        }

        private void processTableRows(IDCO table, XmlNode tableNode)
        {
            ExportCore.WriteDebugLog("In processTableRows:");

            // iterate over rows
            // print rows
            int i = rowStart;
            do
            {

                TDCOLib.DCO row = table.GetChild(i);
                if (row.ObjectType() == Constants.Field)
                {
                    i++;
                    ExportCore.WriteDebugLog("row number :" + i);

                    Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, row);
                    XmlNode dataNode = tableNode.ChildNodes.Item(0);
                    {
                        if (dataNode.Name == Constants.NodeTypeString.SE_DATA)
                        {
                            ExportCore.WriteDebugLog(" fetching column value:" );

                            dataElement.setIsTableColumn(true);
                            dataElement.EvaluateData(dataNode);
                        }
                    }
                }
                Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, Constants.EMPTYSTRING);
            } while (i < rowEnd);
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
