using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SmartExportTemplates.Utils;

namespace SmartExportTemplates.TemplateCore
{
    class ConditionEvaluation
    {
        private string ConditionText = null;
        private List<string> LexParseCondList = null;

        public ConditionEvaluation(string ConditionText)
        {
            this.ConditionText = ConditionText;
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
            // TODO - The main implementation of evaluating a single condition goes here!
            return true;
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
