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

        public List<string> EvaluateCondition(XmlNode ConditionNode)
        {
            List<string> output = new List<string>(); 
            bool ConditionEvaluated = false;

            //Evaluate the IF
            string CondText = ((XmlElement)ConditionNode).GetAttribute(Constants.SE_ATTRIBUTE_COND_TEST);
            ConditionEvaluation conditionEvaluation = new ConditionEvaluation(CondText);

            if (conditionEvaluation.CanEvaluate())
            {
                output.AddRange(getDataStringList(ConditionNode));
                ConditionEvaluated = true;
            }
            
            // Evaluate the ELSIFs if IF has not satisfied
            if (!ConditionEvaluated)
            {
                XmlNodeList elseIfNodeList = ConditionNode.SelectNodes("./" + Constants.NodeTypeString.SE_ELSIF, 
                                                this.templateParser.getNameSpcManager());
                foreach (XmlNode elseIfNode in elseIfNodeList)
                {
                    CondText = ((XmlElement)elseIfNode).GetAttribute(Constants.SE_ATTRIBUTE_COND_TEST);
                    conditionEvaluation = new ConditionEvaluation(CondText);
                    if (conditionEvaluation.CanEvaluate())
                    {
                        output.AddRange(getDataStringList(elseIfNode));
                        ConditionEvaluated = true;
                        break;
                    }
                }
            }

            // Evaluate the Else
            if (!ConditionEvaluated)
            {
                XmlNode elseNode = ConditionNode.SelectSingleNode("./" + Constants.NodeTypeString.SE_ELSE,
                                        this.templateParser.getNameSpcManager());
                if (elseNode != null)
                {
                    output.AddRange(getDataStringList(elseNode));
                    ConditionEvaluated = true;
                } 
            }
            if (!ConditionEvaluated)
            {
                ExportCore.WriteLog(Constants.LOG_PREFIX + "None of the conditions evaluated for the Node with test: " + CondText);
            }
            return output;
        }

        private List<string> getDataStringList(XmlNode parentNode)
        {
            List<string> output = new List<string>();
            DataElement dataElement = new DataElement();

            XmlNodeList dataNodeList = parentNode.SelectNodes("./" + Constants.SE_DATA_NODE_NAME,
                                            this.templateParser.getNameSpcManager());
            foreach (XmlNode dataNode in dataNodeList)
            {
                output.AddRange(dataElement.EvaluateData(dataNode));
            }
            return output;
        }
    }
}
