using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartExportTemplates.TemplateCore
{
    class ConditionEvaluation
    {
        private string ConditionText = null;

        public ConditionEvaluation(string ConditionText)
        {
            this.ConditionText = ConditionText;
        }

        public bool CanEvaluate()
        {
            return true;
        }

         
    }
}
