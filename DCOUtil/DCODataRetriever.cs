using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartExportTemplates.Utils;

namespace SmartExportTemplates.DCOUtil
{
    class DCODataRetriever
    {
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        protected TDCOLib.IDCO DCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_DCO);

        private string getDCOValue(string DCOTree, string pageID)
        {
            // DCO reference in the template file should adhere to a 4 part string [DCO.<doc_type>.<page_type>.<field_name>]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);

            if (dcoArray.Length != 4)
            {
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "DCO reference does not confirm to spec. " +
                    "Expected 4 part string like [DCO.<doc_type>.<page_type>.<field_name>]." +
                    "Found: " + DCOTree);
                throw new System.ArgumentException("DCO reference invalid. Check detailed logs", DCOTree);
            }

            // get the page ID for the page type (in the current document being processed)
            //string pageID = PageTypeDict[dcoArray[2]];

            // get the value of the DCO reference using the page ID and the field name
            string output = null;
            try
            {
                output = DCO.FindChild(pageID).FindChild(dcoArray[3]).Text;
            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "Unable to find DCO reference for the page with ID: " + pageID);
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "Error while reading DCO reference: " + exp.ToString());
            }

            return output;
        }
    }
}
