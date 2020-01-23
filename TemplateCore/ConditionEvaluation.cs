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
            //Split the conditions
            SplitConditions();
            //Apply condition and overwrite the boolen
            ApplyConditions();
            //Evaluate condition and return
            return EvaluateConditions();
        }

        private void SplitConditions()
        {
            this.LexParseCondList = new List<string>(Regex.Split(this.ConditionText, Constants.IF_REF_PATTERN));

        }

        private void ApplyConditions()
        {

        }

         private bool EvaluateConditions()
        {
            return true;
        }
    }
}
