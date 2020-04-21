///////////////using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using System.Diagnostics;

namespace SmartExportTemplates.TemplateCore
{
    class LoopsWithoutDoc:Loops
    {
        TDCOLib.IDCO CurrentDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_CURRENT_DCO);
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        public LoopsWithoutDoc()
        {

        }

        ///       <summary>
        ///       The method Evaluates for loop.
        ///       <param name="loopNode">XML node of Foreach</param>
        ///       </summary>
        public void EvaluateLoop(XmlNode loopNode)
        {
             
            int forEachlevel = getIntValueForEachObjectType(loopNode.Attributes["select"].Value);
            if (forEachlevel == Constants.Document && CurrentDCO.ObjectType() == Constants.Batch )
                EvaluateLoopForFiles(loopNode);
            else
            {
                TemplateParser templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);
                string message = "Problem found at line number : " + templateParser.GetLineNumberForNode(loopNode) + "\n" + "Invalid usage of loops.";
                throw new SmartExportException(message);
            }
               

        }


        public void EvaluateLoopForFiles(XmlNode loopNode)
        {

            Stopwatch sw = Stopwatch.StartNew();

            ExportCore.WriteLog(" Inside EvaluateLoopForFiles");
            DataElement dataElement = new DataElement();
            Conditions conditionEvaluator = new Conditions();
            Tables table = new Tables();
            try
            {
                int forEachlevel = getIntValueForEachObjectType(loopNode.Attributes["select"].Value);
                validateForLoop(forEachlevel, CurrentDCO);

                Dictionary<string, List<string>> filePageMap = (Dictionary<string, List<string>>)Globals.Instance.GetData(Constants.FILE_PAGE_MAP);
                foreach (string file in filePageMap.Keys)
                {

                    //setting the currentIterationDCO , so that it can be used in DCODataRetreiver to get the data.
                    Globals.Instance.SetData(Constants.forLoopString.CURRENTFILE, file);
                    ExportCore.WriteLog(" Current file " + file);

                    foreach (XmlNode node in loopNode.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case Constants.NodeTypeString.SE_IF:
                                conditionEvaluator.EvaluateCondition(node);
                                break;
                            case Constants.NodeTypeString.SE_FOREACH:
                                EvaluateLoopForPagesOfFile(node);
                                break;
                            case Constants.NodeTypeString.SE_ROWS:
                                if (node.Attributes == null || node.Attributes.Count > 0 ||
                                string.IsNullOrEmpty(node.Attributes["tablename"].Value))
                                {
                                    new SmartExportException("Its mandatory to specify the table name when the for-each-rows tag " +
                                        "is used within se:for-each tag for tables.");
                                }
                                table.FetchTable(node);
                                break;
                            case Constants.NodeTypeString.SE_DATA:
                                dataElement.EvaluateData(node);
                                break;
                            default:
                                if (node.NodeType == XmlNodeType.Element)
                                {
                                    ExportCore.WriteLog("Node type [" + ((XmlElement)node).Name + "] not supported. Will be ignored");
                                }
                                break;
                        }
                    }
                    TemplateParser templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);

                    ExportCore.getExportUtil.writeToFile(null);

                    //setting it to empty after every iteration.
                    Globals.Instance.SetData(Constants.forLoopString.CURRENTFILE, Constants.EMPTYSTRING);
                }
            }
            catch (System.Exception exp)
            {
                string message = exp.Message;
                //if the problem was already caught at the child node level the line number
                // information would be already present in the exception message
                if (!message.Contains("Problem found at line number"))
                {
                    TemplateParser templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);
                    message = "Problem found at line number : " + templateParser.GetLineNumberForNode(loopNode) + "\n" + exp.Message;
                }
                //setting it to empty after every iteration.
                Globals.Instance.SetData(Constants.forLoopString.CURRENTFILE, Constants.EMPTYSTRING);
                throw new SmartExportException(message);
            }

            ExportCore.WriteDebugLog(" EvaluateLoopForFiles " + loopNode + "  completed in " + sw.ElapsedMilliseconds + " ms.");

            sw.Stop();
        }



        public void EvaluateLoopForPagesOfFile(XmlNode loopNode)
        {

            Stopwatch sw = Stopwatch.StartNew();


            DataElement dataElement = new DataElement();
            Conditions conditionEvaluator = new Conditions();
            Tables table = new Tables();
            try
            {

                Dictionary<string, List<string>> filePageMap = (Dictionary<string, List<string>>)Globals.Instance.GetData(Constants.FILE_PAGE_MAP);
                string file = (string)Globals.Instance.GetData(Constants.forLoopString.CURRENTFILE);
                List<string> pages = filePageMap[file];
                foreach (string page in pages)
                {
                    TDCOLib.IDCO DCO = CurrentDCO.FindChild(page);
                    Globals.Instance.SetData(Constants.GE_CURRENT_DCO, DCO);
                    foreach (XmlNode node in loopNode.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case Constants.NodeTypeString.SE_IF:
                                conditionEvaluator.EvaluateCondition(node);
                                break;

                            case Constants.NodeTypeString.SE_ROWS:
                                if (node.Attributes == null || node.Attributes.Count > 0 ||
                                string.IsNullOrEmpty(node.Attributes["tablename"].Value))
                                {
                                    new SmartExportException("Its mandatory to specify the table name when the for-each-rows tag " +
                                        "is used within se:for-each tag for tables.");
                                }
                                table.FetchTable(node);
                                break;
                            case Constants.NodeTypeString.SE_DATA:
                                dataElement.EvaluateData(node);
                                break;
                            default:
                                if (node.NodeType == XmlNodeType.Element)
                                {
                                    ExportCore.WriteLog("Node type [" + ((XmlElement)node).Name + "] not supported. Will be ignored");
                                }
                                break;
                        }
                    }
                    Globals.Instance.SetData(Constants.GE_CURRENT_DCO, CurrentDCO);
                   
                }
            }
            catch (System.Exception exp)
            {
                string message = exp.Message;
                //if the problem was already caught at the child node level the line number
                // information would be already present in the exception message
                if (!message.Contains("Problem found at line number"))
                {
                    TemplateParser templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);
                    message = "Problem found at line number : " + templateParser.GetLineNumberForNode(loopNode) + "\n" + exp.Message;
                }
                //setting it to empty after every iteration.
                Globals.Instance.SetData(Constants.forLoopString.CURRENTFILE, Constants.EMPTYSTRING);
                throw new SmartExportException(message);
            }

            ExportCore.WriteDebugLog(" EvaluateLoopForFiles " + loopNode + "  completed in " + sw.ElapsedMilliseconds + " ms.");

            sw.Stop();
        }





    }
}
