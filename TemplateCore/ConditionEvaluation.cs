using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SmartExportTemplates.Utils;
using SmartExportTemplates.DCOUtil;
using Datacap.Global;

namespace SmartExportTemplates.TemplateCore
{
    class ConditionEvaluation
    {
        private string ConditionText = null;
        private List<string> LexParseCondList = null;
        private DCODataRetriever dCODataRetriever =null;
        private DataTypeChecker dataTypeChecker = new DataTypeChecker();
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        bool projectHasDoc = (bool)Globals.Instance.GetData(Constants.PROJECT_HAS_DOC);
        string pattern;

        public ConditionEvaluation(string ConditionText)
        {
            this.ConditionText = ConditionText;            
        }

        public bool CanEvaluate()
        {
            if (projectHasDoc)
            {
                dCODataRetriever = new DCODataRetriever();
                pattern = Constants.DCO_REF_PATTERN;
            }
            else
            {
                dCODataRetriever = new DCODataRetrieverWithoutDoc();
                pattern = Constants.DCO_REF_PATTERN_NO_DOC;
            }
            //Parse Conditions
            ParseConditions();
            //Apply condition and overwrite the boolen
            ApplyConditionsAndReplace();
            //Evaluate condition and return
            return EvaluateConditions();
        }

        private void ParseConditions()
        {
            //Split the conditions into a set of individual conditions.
            //Example: "a>b and c==d or z<y and m==n" is stored in the below array as
            // ["a>b", "and", "c==d", "or", "z<y", "and", "m==n"]
            this.LexParseCondList = new List<string>(Regex.Split(this.ConditionText, Constants.IF_REF_PATTERN));
            //Validate if the list is and ODD number. 
            if ((this.LexParseCondList.Count & 1) == 0)
            {
                throw new SmartExportException("Unsupported syntax. Check documentation: " + ConditionText);
            }
        }

        private void ApplyConditionsAndReplace()
        {
            // Evaluate conditions and store the boolean back onto the list
            List<string> tmpList = this.LexParseCondList.Select(x => ReplaceConditions(x)).ToList();
            this.LexParseCondList = tmpList;
        }

        private string ReplaceConditions(string conditionText)
        {
            string output = conditionText;
            //Don't replace "and/or", evaluate the rest as individual conditions
            if (!Regex.Match(conditionText, Constants.IF_REF_PATTERN).Success)
            {
                 output = EvaluateIndividualCondition(conditionText) ? "true" : "false";
            }
            return output;
        }

        private bool EvaluateIndividualCondition(string conditionText)
        {
            ExportCore.WriteLog(" Evaluating condition " + conditionText);

            bool response = false;
            List<string> operands
                = new List<string>(Regex.Split(conditionText, Constants.ALLOWED_OPERATORS));

            // check if there are 2 operands
            if (3 != operands.Count)
            {
                throw new SmartExportException("Unsupported syntax. Check documentation: " + conditionText);
            }
            //check if allowed operator are used
            if (!Regex.Match(operands[1].Trim(), Constants.ALLOWED_OPERATORS).Success)
            {
                throw new SmartExportException("Unsupported syntax. Check documentation: " + conditionText);
            }
            Regex rx = new Regex(pattern);
            for (int i = 0; i < operands.Count; i++)
            {
                //replace DCO referencing expressions
                if (rx.IsMatch(operands[i].Trim()))
                {
                    string expr = operands[i];
                    operands[i] = dCODataRetriever.getDCOValue(operands[i].Trim());
                    if ("" == operands[i])
                    {
                        ExportCore.WriteLog("Could not find value for  " + expr + " in " + conditionText);
                        return false;
                    }
                }
                else if(Constants.ConditionString.DOCUMENT_TYPE == operands[i].Trim())
                {
                    operands[i] = dCODataRetriever.getDocumentType();
                }
                else if (Constants.ConditionString.PAGE_TYPE == operands[i].Trim())
                {
                    operands[i] = dCODataRetriever.getPageType();
                }
                else if (Constants.ConditionString.TABLE_TYPE == operands[i].Trim())
                {
                    operands[i] = dCODataRetriever.getTableType();
                }
                else if (Constants.ConditionString.FILE_PAGE_TYPES == operands[i].Trim())
                {
                    operands[i] = dCODataRetriever.getPageTypesInFile();
                }
                else
                {
                    operands[i] = operands[i].Trim();
                }
            }


            //check if comparisons are done for same data types
            string operandOneType = dataTypeChecker.getType(operands[0].Trim());
            string operandTwoType = dataTypeChecker.getType(operands[2].Trim());

            // allow operations on numeric opernads even if they are not of the same data type
            if (operandOneType != operandTwoType && dataTypeChecker.numericTypes.Contains(operandOneType)
                && dataTypeChecker.numericTypes.Contains(operandTwoType))
            {
                operandTwoType = operandOneType = castNumericDataTypes(operandOneType, operandTwoType);
            }
            if (operandOneType != operandTwoType)
            {
                throw new SmartExportException("Invalid comparisons in : "
                    + conditionText + " " + operandOneType + " " + operands[1] + " " + operandTwoType);
            }
            try
            {
                response = ExpressionEvaluator.evaluateExpression(operands[0].Trim(), operands[2].Trim(), operandOneType, operands[1].Trim());
            }
            catch (Exception exp)
            {
                // Evaluating conditions using this technique is risky (injections)
                // Moreover, this API is deprecated. This is for the timebeing (to prototype and define the POV)
                // If conditions do not evaluate properly, write log and skip the condition
                // TODO: Lexical parser to get the conditions and transform them to c# code constructs
                string message = "Condition evalution failed for condition:" + conditionText;
                ExportCore.WriteErrorLog(message);
                ExportCore.WriteErrorLog(exp.StackTrace);
                throw new SmartExportException(message);

            }
            ExportCore.WriteLog(" Condition  " + conditionText + " is evaluated as " + response);

            return response;
        }

        private string castNumericDataTypes(string operandOneType, string operandTwoType)
        {
            string type = operandOneType;
            if (operandOneType == Constants.DataTypeString.DOUBLE || operandTwoType == Constants.DataTypeString.DOUBLE)
            {
                type = Constants.DataTypeString.DOUBLE;
            }
             
            return type;
        }

         

        private bool EvaluateConditions()
        {
            //TODO: There should be a better way to do this... For the moment
            if( this.LexParseCondList.Count == 1
                && (this.LexParseCondList.Contains("true") || this.LexParseCondList.Contains("false"))
                 )
            {
                return Boolean.Parse(LexParseCondList.ElementAt(0));
            }
            if (this.LexParseCondList.Count < 3 )
            {
                // Min 3 items needed to evaluate
                return false;
            }
            // Apply logical operators on the operands from left to right until the list is exhausted. 
            // Parenthesis is not supported in release 1 and hence the result of the first two operands on the operator is 
            // taken as the base for the next operator/operand that follows. 
            // "a and b or c and d" evaluated as "((a and b) or c) and d)". Future releases will support parenthesis
            string output = "false";
            int rolling_index = 0;
            string left = this.LexParseCondList.ElementAt(rolling_index);
            string right = this.LexParseCondList.ElementAt(rolling_index + 2);
            string log_operator = this.LexParseCondList.ElementAt(rolling_index + 1);
            do
            {
                output = applyCondition(left, right, log_operator);
                if ((rolling_index + 4) >= this.LexParseCondList.Count)
                {
                    break;
                }
                rolling_index += 2;
                left = output;
                right = this.LexParseCondList.ElementAt(rolling_index + 2);
                log_operator = this.LexParseCondList.ElementAt(rolling_index + 1);
            } while (true);

            return (output.Trim().ToLower().Equals("true") ? true : false);
        }

        private string applyCondition(string operand1, string operand2, string log_operator)
        {
            //TODO: There should be a better way to do this. For the moment!
            bool left = Boolean.Parse(operand1);
            bool right = Boolean.Parse(operand2);
            string output = "false";

            switch (log_operator.Trim().ToLower())
            {
                case Constants.CondLogOperators.COND_LOG_AND:
                    output = (left && right) ? "true" : "false";
                    break;
                case Constants.CondLogOperators.COND_LOG_OR:
                    output = (left || right) ? "true" : "false";
                    break;
                default:
                    // Only "and" and "or" is supported for now.
                    throw new SmartExportException("Logical operators other than 'and' and 'or' are not supported. Check the condition: " + this.ConditionText);
            }

            return output;
        }

    }
}
