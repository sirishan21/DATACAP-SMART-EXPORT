using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SmartExportTemplates.Utils;
using SmartExportTemplates.DCOUtil;
using Validations;
namespace SmartExportTemplates.TemplateCore
{
    class ConditionEvaluation
    {
        private string ConditionText = null;
        private List<string> LexParseCondList = null;
        private DCODataRetriever dCODataRetriever = new DCODataRetriever();
        private DataTypeChecker dataTypeChecker = new DataTypeChecker();
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        Dictionary<string, List<string>> operatorTypeMap = new Dictionary<string, List<string>>();
        public ConditionEvaluation(string ConditionText)
        {
            this.ConditionText = ConditionText;
            operatorTypeMap.Add(Constants.Operators.EQUALS, new List<string>()
            {
               Constants.DataTypeString.BOOL,Constants.DataTypeString.DATE_TIME,Constants.DataTypeString.DOUBLE,
                Constants.DataTypeString.INT32,Constants.DataTypeString.INT64,Constants.DataTypeString.STRING
            });
            operatorTypeMap.Add(Constants.Operators.GREATER_THAN, new List<string>()
            {
              Constants.DataTypeString.DATE_TIME,Constants.DataTypeString.DOUBLE,
                Constants.DataTypeString.INT32,Constants.DataTypeString.INT64
            });
            operatorTypeMap.Add(Constants.Operators.LESSER_THAN, new List<string>()
            {
              Constants.DataTypeString.DATE_TIME,Constants.DataTypeString.DOUBLE,
                Constants.DataTypeString.INT32,Constants.DataTypeString.INT64
            });
        }

        public bool CanEvaluate()
        {
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
            ExportCore.WriteLog(Constants.GE_LOG_PREFIX + " Evaluating condition " + conditionText);

            bool response = false;
            List<string> operands
                = new List<string>(Regex.Split(this.ConditionText, Constants.ALLOWED_OPERATORS));

            // check if there are 2 operands
            if (2 != operands.Count)
            {
                throw new SmartExportException("Unsupported syntax. Check documentation: " + conditionText);
            }
            string op = this.ConditionText.Replace(operands[0], "").Replace(operands[1], "");
            //check if allowed operator are used
            if (!Regex.Match(op, Constants.ALLOWED_OPERATORS).Success)
            {
                throw new SmartExportException("Unsupported syntax. Check documentation: " + conditionText);
            }
            //replace DCO referencing expressions
            Regex rx = new Regex(Constants.DCO_REF_PATTERN);
            for (int i = 0; i < operands.Count; i++)
            {
                if (rx.IsMatch(operands[i]))
                {
                    string expr = operands[i];
                    operands[i] = dCODataRetriever.getDCOValue(operands[i]);
                    if ("" == operands[i])
                    {
                        ExportCore.WriteLog(Constants.GE_LOG_PREFIX +
                            "Could not find value for  " + expr + " in " + conditionText);
                        return false;
                    }
                }
            }


            //check if comparisons are done for same data types
            string operandOneType = dataTypeChecker.getType(operands[0]);
            string operandTwoType = dataTypeChecker.getType(operands[1]);
            if (operandOneType != operandTwoType)
            {
                throw new SmartExportException("Invalid comparisons in : "
                    + conditionText + " " + operandOneType + " " + op + " " + operandTwoType);
            }

            checkIfOperatorIsApplicable(operandOneType, op);
            try
            {
                response = evaluateExpression(operands, operandOneType, op);
            }
            catch (Exception exp)
            {
                // Evaluating conditions using this technique is risky (injections)
                // Moreover, this API is deprecated. This is for the timebeing (to prototype and define the POV)
                // If conditions do not evaluate properly, write log and skip the condition
                // TODO: Lexical parser to get the conditions and transform them to c# code constructs
                throw new SmartExportException("Condition evalution failed for condition:" + conditionText);
            }
            ExportCore.WriteLog(Constants.GE_LOG_PREFIX + " Condition  " + conditionText + " is evaluated as " + response);

            return response;
        }

        private bool evaluateExpression(List<string> operands, string type, string op)
        {
            bool response = false;
            if ((operands[0] == "" || operands[1] == "") && type != Constants.DataTypeString.STRING)
            {
                return false;
            }
            switch (type + op)
            {
                case Constants.DataTypeString.BOOL + Constants.Operators.EQUALS:
                    response = bool.Parse(operands[0]) == bool.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.STRING + Constants.Operators.EQUALS:
                    response = operands[0].Equals(operands[1], StringComparison.OrdinalIgnoreCase);
                    break;
                case Constants.DataTypeString.INT32 + Constants.Operators.EQUALS:
                    response = Int32.Parse(operands[0]) == Int32.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.INT32 + Constants.Operators.GREATER_THAN:
                    response = Int32.Parse(operands[0]) > Int32.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.INT32 + Constants.Operators.LESSER_THAN:
                    response = Int32.Parse(operands[0]) < Int32.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.INT64 + Constants.Operators.EQUALS:
                    response = Int64.Parse(operands[0]) == Int64.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.INT64 + Constants.Operators.GREATER_THAN:
                    response = Int64.Parse(operands[0]) > Int64.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.INT64 + Constants.Operators.LESSER_THAN:
                    response = Int64.Parse(operands[0]) < Int64.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.DOUBLE + Constants.Operators.EQUALS:
                    response = double.Parse(operands[0]) == double.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.DOUBLE + Constants.Operators.GREATER_THAN:
                    response = double.Parse(operands[0]) > double.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.DOUBLE + Constants.Operators.LESSER_THAN:
                    response = double.Parse(operands[0]) < double.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.DATE_TIME + Constants.Operators.EQUALS:
                    response = DateTime.Parse(operands[0]) == DateTime.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.DATE_TIME + Constants.Operators.GREATER_THAN:
                    response = DateTime.Parse(operands[0]) > DateTime.Parse(operands[1]);
                    break;
                case Constants.DataTypeString.DATE_TIME + Constants.Operators.LESSER_THAN:
                    response = DateTime.Parse(operands[0]) < DateTime.Parse(operands[1]);
                    break;
            }
            return response;
        }

        private void checkIfOperatorIsApplicable(string operandOneType, string op)
        {
            List<string> types = operatorTypeMap[op];
            if (!types.Contains(operandOneType))
            {
                throw new SmartExportException("The operator " + op + " is not applicable for type " + operandOneType);
            }
        }



        private bool EvaluateConditions()
        {
            //TODO: There should be a better way to do this... For the moment
            if (this.LexParseCondList.Count < 3)
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
