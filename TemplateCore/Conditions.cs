﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using SmartExportTemplates.TemplateCore;
using System.Diagnostics;

namespace SmartExportTemplates
{
    class Conditions
    {
        TemplateParser templateParser = null;
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);

        public Conditions()
        {
            this.templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);
        }

        public List<string> EvaluateCondition(XmlNode ConditionNode)
        {
            Stopwatch sw = Stopwatch.StartNew();

            List<string> output = new List<string>();
            bool ConditionEvaluated = false;

            //Evaluate the IF
            string CondText = ((XmlElement)ConditionNode).GetAttribute(Constants.SE_ATTRIBUTE_COND_TEST);
            ConditionEvaluation conditionEvaluation = new ConditionEvaluation(CondText);

            if (conditionEvaluation.CanEvaluate())
            {
                output.AddRange(processChildNodes(ConditionNode));
                ConditionEvaluated = true;
            }
            
            // Evaluate the ELSIFs if IF has not satisfied
            if (!ConditionEvaluated)
            {
                XmlNodeList elseIfNodeList = ConditionNode.ChildNodes;
                foreach (XmlNode elseIfNode in elseIfNodeList)
                {
                    if (elseIfNode.Name == Constants.NodeTypeString.SE_ELSIF)
                    {
                        CondText = ((XmlElement)elseIfNode).GetAttribute(Constants.SE_ATTRIBUTE_COND_TEST);
                        conditionEvaluation = new ConditionEvaluation(CondText);
                        if (conditionEvaluation.CanEvaluate())
                        {
                            output.AddRange(processChildNodes(elseIfNode));
                            ConditionEvaluated = true;
                            break;
                        }
                        // reaching else node indicates there are no more nodes with node name ELSIF
                        if (elseIfNode.Name == Constants.NodeTypeString.SE_ELSE)
                        {
                            break;
                        }
                    }
                }
            }

            // Evaluate the Else
            if (!ConditionEvaluated)
            {
                XmlNodeList elseNodeList = ConditionNode.ChildNodes;
                foreach (XmlNode elseNode in elseNodeList)
                {
                    if ( elseNode.Name == Constants.NodeTypeString.SE_ELSE)
                    {
                        output.AddRange(processChildNodes(elseNode));
                        ConditionEvaluated = true;
                        break;
                    }

                }

            }
            if (!ConditionEvaluated)
            {
                ExportCore.WriteLog("None of the conditions evaluated for the Node with test: " + CondText);
            }

            ExportCore.WriteLog(" EvaluateCondition("+ConditionNode+") completed in " + sw.ElapsedMilliseconds + " ms.");

            sw.Stop();

            return output;
        }

        private List<string> processChildNodes(XmlNode parentNode)
        {
            List<string> output = new List<string>();
            DataElement dataElement = new DataElement();

            XmlNodeList childNodes = parentNode.ChildNodes;
            foreach (XmlNode childNode in childNodes)
            {
                if (childNode.Name == Constants.NodeTypeString.SE_DATA)
                    output.AddRange(dataElement.EvaluateData(childNode));
                else if (childNode.Name == Constants.NodeTypeString.SE_IF)
                    output.AddRange(new Conditions().EvaluateCondition(childNode));
                else if (childNode.Name == Constants.NodeTypeString.SE_FOREACH)
                    output.AddRange(new Loops().EvaluateLoop(childNode));
            }

            return output;
        }
    }
}
