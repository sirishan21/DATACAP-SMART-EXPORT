using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using System.Diagnostics;

namespace SmartExportTemplates.TemplateCore
{
    class Loops
    {
        protected TDCOLib.IDCO CurrentDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_CURRENT_DCO);
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        private int nestingLevel = 0;
        public Loops()
        {

        }

        ///       <summary>
        ///       The method Evaluates for loop.
        ///       <param name="loopNode">XML node of Foreach</param>
        ///       </summary>
        public void EvaluateLoop(XmlNode loopNode)
        {
            TDCOLib.IDCO DCO = null;
            if (Globals.Instance.ContainsKey(Constants.forLoopString.CURRENTITERATIONDCO)
                && Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO) is TDCOLib.IDCO)
            {
                DCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO);
            }
            if (DCO == null)
            {
                DCO = CurrentDCO;
            }
            EvaluateLoop(loopNode, DCO);

        }

        ///       <summary>
        ///       The method Evaluates for loop.
        ///       <param name="loopNode">XML node of Foreach</param>
        ///       <param name="DCO">Current iteration DCO of the parent for-each loop</param>
        ///       </summary>
        public void EvaluateLoop(XmlNode loopNode, TDCOLib.IDCO DCO)
        {

            Stopwatch sw = Stopwatch.StartNew();


            DataElement dataElement = new DataElement();
            Conditions conditionEvaluator = new Conditions();
            Tables table = new Tables();
            try
            {
                int forEachlevel = getIntValueForEachObjectType(loopNode.Attributes["select"].Value);
                nestingLevel = setAndValidateNestingLevel(loopNode);
                validateForLoop(forEachlevel, DCO);

                for (int i = 0; i < DCO.NumOfChildren(); i++)
                {

                    //setting the currentIterationDCO , so that it can be used in DCODataRetreiver to get the data.
                    Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, DCO.GetChild(i));

                    foreach (XmlNode node in loopNode.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case Constants.NodeTypeString.SE_IF:
                                conditionEvaluator.EvaluateCondition(node);
                                break;
                            case Constants.NodeTypeString.SE_FOREACH:
                                Loops loopEvaluator = new Loops();
                                loopEvaluator.EvaluateLoop(node, DCO.GetChild(i));
                                break;
                            case Constants.NodeTypeString.SE_ROWS:
                                if (node.Attributes == null || node.Attributes.Count > 0 ||
                                string.IsNullOrEmpty(node.Attributes["tablename"].Value))
                                {                              
                                    new SmartExportException("Its mandatory to specify the table name when the for-each-rows tag " +
                                        "is used within se:for-each tag for tables.");
                                }
                                if (node.Attributes["tablename"].Value == DCO.GetChild(i).ID)
                                {
                                    table.FetchTable(node);
                                }                                
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
                    //setting it to empty after every iteration.
                    Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, Constants.EMPTYSTRING);
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
                Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, Constants.EMPTYSTRING);
                throw new SmartExportException(message);
            }

            ExportCore.WriteDebugLog(" EvaluateLoop " + loopNode + "  completed in " + sw.ElapsedMilliseconds + " ms.");

            sw.Stop();
        }

        ///       <summary>
        ///       The method determines the nesting level and also validates the same.
        ///       Nesting Level O indicates the other most loop.
        ///       There can be only 3 levels of valid nesting - 0,1,2.
        ///       <param name="loopNode">for-each tag node</param>
        ///       </summary>
        protected int setAndValidateNestingLevel(XmlNode loopNode)
        {

            int nestingLevel = -1;

            while (null != loopNode)
            {
                nestingLevel++;
                XmlNode parentNode = loopNode.ParentNode;               
                if(null != parentNode && parentNode.Name!= Constants.NodeTypeString.SE_FOREACH)
                {
                    
                    break;
                }
                else if (null != parentNode && parentNode.Name == Constants.NodeTypeString.SE_FOREACH)
                {
                    int currentForEachlevel = getIntValueForEachObjectType(loopNode.Attributes["select"].Value);
                    int parentForEachlevel = getIntValueForEachObjectType(parentNode.Attributes["select"].Value);
                    if (currentForEachlevel != parentForEachlevel + 1 )
                    {
                        throw new SmartExportException(" ForEach nesting is invalid.");
                    }
                }
                loopNode = parentNode;                
            }
           
            return nestingLevel;
        }


        ///       <summary>
        ///       The method returns integer value of the object type passed.
        ///       <param name="objectType">String value of the level</param>
        ///       </summary>
        protected int getIntValueForEachObjectType(String objectType){
            int output = 4;
            switch(objectType.ToUpper()){
            case Constants.forLoopString.BATCH:
                    output = Constants.Batch;
                    break;
            case Constants.forLoopString.DOCUMENT:
                    output = Constants.Document;
                    break;
            case Constants.forLoopString.PAGE:
                    output = Constants.Page;
                    break;
            case Constants.forLoopString.FIELD:
            case Constants.forLoopString.TABLE:
                    output = Constants.Field;
                    break;
            }
            return output;
        }

        ///       <summary>
        ///       The method validates for loop expression.
        ///       <param name="forEachlevel">integer value of the level</param>
        ///       </summary>
        protected void validateForLoop(int forEachlevel, TDCOLib.IDCO DCO)
        {
            if(DCO == null)
            {
                throw new SmartExportException(" DCO associated with the for-each loop cannot be determined.");
            }
            if (forEachlevel == 4 || forEachlevel == Constants.Batch)
            {
                throw new SmartExportException(" Assigned level of ForEach loop is wrong"); 
            }
           if(forEachlevel == DCO.ObjectType()){
                throw new SmartExportException(" ForEach loop level must not be same to the Datacap assigned level"); 
            }
           if(!(forEachlevel == (DCO.ObjectType()+1))){
                throw new SmartExportException(" ForEach loop must be one level lower from the Datacap assigned level "); 
            }
        }

    }
}
