﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;

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
        ///       <param name="output">List of strings to be printed in the output file.</param>
        ///       </summary>
        public void EvaluateLoop(XmlNode loopNode,  List<string> output)
        {

            DataElement dataElement = new DataElement();
            Conditions conditionEvaluator = new Conditions();

            int forEachlevel = getIntValueForEachObjectType(loopNode.Attributes["select"].Value);
            validateForLoop(forEachlevel, CurrentDCO);

            for (int i = 0; i < CurrentDCO.NumOfChildren(); i++){

               //setting the currentIterationDCO , so that it can be used in DCODataRetreiver to get the data.
               Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, CurrentDCO.GetChild(i));

                foreach (XmlNode node in loopNode.ChildNodes)
                {
                    switch(node.Name){
                        case Constants.NodeTypeString.SE_IF:
                             conditionEvaluator.EvaluateCondition(node, output);
                            break;
                        case Constants.NodeTypeString.SE_FOREACH:
                            Loops loopEvaluator = new Loops();
                            loopEvaluator.EvaluateInnerLoop(node, CurrentDCO.GetChild(i),output);
                            break;
                        case Constants.NodeTypeString.SE_DATA:
                            output.AddRange(dataElement.EvaluateData(node));
                            break;
                        default:
                            if (node.NodeType == XmlNodeType.Element)
                            {
                                ExportCore.WriteLog( "Node type [" + ((XmlElement)node).Name + "] not supported. Will be ignored");
                            }
                            break;
                    }
                }
               //setting it to empty after every iteration.
               Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, Constants.EMPTYSTRING);
            }
            
        }

        ///       <summary>
        ///       The method Evaluates for loop.
        ///       <param name="loopNode">XML node of Foreach</param>
        ///       <param name="DCO">Current iteration DCO of the parent for-each loop</param>
        ///       <param name="output">List of strings to be printed in the output file.</param>
        ///       </summary>
        public void EvaluateInnerLoop(XmlNode loopNode, TDCOLib.IDCO DCO, List<string> output)
        {

            DataElement dataElement = new DataElement();
            Conditions conditionEvaluator = new Conditions();

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
                            conditionEvaluator.EvaluateCondition(node, output);
                            break;
                        case Constants.NodeTypeString.SE_FOREACH:
                            Loops loopEvaluator = new Loops();
                            loopEvaluator.EvaluateInnerLoop(node, DCO.GetChild(i), output);
                            break;
                        case Constants.NodeTypeString.SE_DATA:
                            output.AddRange(dataElement.EvaluateData(node));
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

        ///       <summary>
        ///       The method determines the nesting level and also validates the same.
        ///       Nesting Level O indicates the other most loop.
        ///       There can be only 3 levels of valid nesting - 0,1,2.
        ///       <param name="loopNode">for-each tag node</param>
        ///       </summary>
        private int setAndValidateNestingLevel(XmlNode loopNode)
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
                    if (currentForEachlevel != parentForEachlevel + 1)
                    {
                        throw new SmartExportException(Constants.LOG_PREFIX
                            + " ForEach nesting is invalid.");
                    }
                }
                loopNode = parentNode;                
            }
           
            return nestingLevel;
        }

        ///       <summary>
        ///       The method determines the DCO corresponding nesting level.
        ///       </summary>
        private TDCOLib.IDCO   getDCOForNestingLevel()
        {
            TDCOLib.IDCO DCO = null;

            switch (nestingLevel)
            {
                case 0:
                    DCO = CurrentDCO;
                    break;
                case 1:
                    if (0 != CurrentDCO.NumOfChildren())
                    {
                        DCO = CurrentDCO.GetChild(0);
                    }
                    break;
                case 2:
                    if (0 != CurrentDCO.NumOfChildren())
                    {
                        TDCOLib.DCO childDCO = CurrentDCO.GetChild(0);
                        if (0 != childDCO.NumOfChildren())
                        {
                            DCO = childDCO.GetChild(0);
                        }
                    }
                    break;
            }
            
            return DCO;
        }

        ///       <summary>
        ///       The method returns integer value of the object type passed.
        ///       <param name="objectType">String value of the level</param>
        ///       </summary>
        private int getIntValueForEachObjectType(String objectType){
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
                    output = Constants.Field;
                    break;
            }
            return output;
        }

        ///       <summary>
        ///       The method validates for loop expression.
        ///       <param name="forEachlevel">integer value of the level</param>
        ///       </summary>
        private void validateForLoop(int forEachlevel, TDCOLib.IDCO DCO)
        {
            if(nestingLevel == -1 || nestingLevel > 2)
            {
                throw new SmartExportException(Constants.LOG_PREFIX + " Nesting level of ForEach loop is invalid");
            }
            if(DCO == null)
            {
                throw new SmartExportException(Constants.LOG_PREFIX + " DCO associated with the for-each loop cannot be determined.");
            }
            if (forEachlevel == 4 || forEachlevel == Constants.Batch)
            {
                throw new SmartExportException(Constants.LOG_PREFIX + " Assigned level of ForEach loop is wrong"); 
            }
           if(forEachlevel == DCO.ObjectType()){
                throw new SmartExportException(Constants.LOG_PREFIX + " ForEach loop level must not be same to the Datacap assigned level"); 
            }
           if(!(forEachlevel == (DCO.ObjectType()+1))){
                throw new SmartExportException(Constants.LOG_PREFIX + " ForEach loop must be one level lower from the Datacap assigned level "); 
            }
        }

    }
}
