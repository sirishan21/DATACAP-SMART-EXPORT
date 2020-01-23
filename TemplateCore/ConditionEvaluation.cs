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
            //TODO - From left to right, apply the logical and/or operators on the individual condition results and return a single bool
            return true;
        }

    }
}
