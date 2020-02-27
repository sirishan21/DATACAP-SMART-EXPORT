﻿using System;
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
        private dcSmart.SmartNav SmartNav = (dcSmart.SmartNav)Globals.Instance.GetData(Constants.GE_SMART_NAV);
        private SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);


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
                        case Constants.SE_IMAGE_NAME:
                            //text.Append(Globals.Instance.GetData(Constants.IMAGE_NAME));
                            text.Append(SmartNav.MetaWord("@P.ScanSrcPath"));
                            break;
                        case Constants.SE_VALUE_NODE_NAME:
                            text.Append(dCODataRetriever.getDCOValue(node.Attributes["select"].Value));
                            break;
                        case Constants.SE_SMART_PARAM_NODE_NAME:
                           text.Append(SmartNav.MetaWord(Constants.SMARTP_AT + node.InnerText.Trim()));
                           ExportCore.WriteDebugLog("smart param value for '"+ node.InnerText.Trim() + "' is " + text);
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