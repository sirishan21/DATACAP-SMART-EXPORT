using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using SmartExportTemplates.TemplateCore;

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

        public void EvaluateCondition(XmlNode ConditionNode, List<string> output)
        {
            bool ConditionEvaluated = false;

            //Evaluate the IF
            string CondText = ((XmlElement)ConditionNode).GetAttribute(Constants.SE_ATTRIBUTE_COND_TEST);
            ConditionEvaluation conditionEvaluation = new ConditionEvaluation(CondText);

            if (conditionEvaluation.CanEvaluate())
            {
                processChildNodes(ConditionNode, output);
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
                            processChildNodes(elseIfNode, output);
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
                        processChildNodes(elseNode, output);
                        ConditionEvaluated = true;
                        break;
                    }

                }

            }
            if (!ConditionEvaluated)
            {
                ExportCore.WriteLog(Constants.LOG_PREFIX + "None of the conditions evaluated for the Node with test: " + CondText);
            }
        }

        private void processChildNodes(XmlNode parentNode, List<string> output)
        {
            DataElement dataElement = new DataElement();

            XmlNodeList childNodes = parentNode.ChildNodes;
            foreach (XmlNode childNode in childNodes)
            {
                if (childNode.Name == Constants.NodeTypeString.SE_DATA)
                    output.AddRange(dataElement.EvaluateData(childNode));
                else if (childNode.Name == Constants.NodeTypeString.SE_IF)
                    new Conditions().EvaluateCondition(childNode, output);
                else if (childNode.Name == Constants.NodeTypeString.SE_FOREACH)
                    new Loops().EvaluateLoop(childNode, output);
            }
        }
    }
}
