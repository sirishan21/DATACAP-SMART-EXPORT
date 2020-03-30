using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using SmartExportTemplates.DCOUtil;
using System.Diagnostics;

namespace SmartExportTemplates.TemplateCore
{
    class DataElement
    {
        private DCODataRetriever dCODataRetriever = new DCODataRetriever();
        private dcSmart.SmartNav SmartNav = (dcSmart.SmartNav)Globals.Instance.GetData(Constants.GE_SMART_NAV);
        private SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        private bool isTableColumn = false;
        private string columnSeparator = "";


        public DataElement()
        {

        }

        public void EvaluateData(XmlNode DataNode)
        {

            Stopwatch sw = Stopwatch.StartNew();

            string NodeName = ((XmlElement)DataNode).Name;

            if (DataNode.HasChildNodes)
            {
                StringBuilder text = new StringBuilder(Constants.EMPTYSTRING);
                foreach (XmlNode node in DataNode.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case Constants.TEXT_NODE_NAME:
                            text.Append(node.Value.Trim());
                            break;
                        case Constants.SE_TAB_NODE_NAME:
                            text.Append(Constants.TAB_SPACE);
                            break;
                        case Constants.SE_NEW_LINE_NODE_NAME:
                            text.Append(Constants.NEW_LINE);
                            break;
                        case Constants.SE_COMMA_NODE_NAME:
                            if (isTableColumn)
                                columnSeparator=Constants.COMMA;
                            text.Append(Constants.COMMA);
                            break;
                        case Constants.SE_VALUE_NODE_NAME:
                            string value = "";
                            if (isTableColumn)
                            {
                                value = dCODataRetriever.getColumnValueForRow(node.Attributes["select"].Value);
                                text.Append(ExportCore.getExportUtil.escapeString(value,columnSeparator));
                            }
                            else
                            {
                                value = dCODataRetriever.getDCOValue(node.Attributes["select"].Value).Trim();
                                text.Append(ExportCore.getExportUtil.escapeString(value));
                            }
                            break;
                        case Constants.SE_SMART_PARAM_NODE_NAME:
                           text.Append(SmartNav.MetaWord(Constants.SMARTP_AT + node.InnerText.Trim()).Trim());
                           ExportCore.WriteDebugLog("smart param value for '"+ node.InnerText.Trim() + "' is " + text);
                           break;
                        default:
                            ExportCore.WriteInfoLog("Node type [" + node.Name + "] is not supported inside data node. Will be ignored ");
                            break;
                    }
                }
                if (text.Length > 0)
                {
                    ExportCore.getExportUtil.addToOutPutList(text.ToString());
                }

            }

            ExportCore.WriteDebugLog(" EvaluateData(" + DataNode + ") completed in " + sw.ElapsedMilliseconds + " ms.");

            sw.Stop();

        }

        public void setIsTableColumn(bool IsTableColumn)
        {
            this.isTableColumn = IsTableColumn;
        }
    }
}