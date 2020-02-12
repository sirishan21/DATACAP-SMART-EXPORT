using System;
using System.Collections.Generic;
using System.Xml;
using SmartExportTemplates.Utils;

namespace SmartExportTemplates.DCOUtil
{
    class DCODataRetriever
    {
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        protected TDCOLib.IDCO CurrentDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_CURRENT_DCO);
        protected TDCOLib.IDCO DCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_DCO);

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified, from the current DCO.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <returns>The value corresponding to the DCO expression specified, from the current DCO.</returns>
        ///       </summary>
         public string getDCOValue(string DCOTree)        
        {
            string output = "";
            TDCOLib.IDCO currentIterationDCO = null;
            int objectType = CurrentDCO.ObjectType();

            //If the call is from ForEach, this will be having a currentIterationDCO object.
            if(!Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO).Equals(Constants.EMPTYSTRING)){
                currentIterationDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO);
                objectType = currentIterationDCO.ObjectType();
            }  
            
            switch (objectType)
            {
                case Constants.Batch:
                    string message = "Unable to find DCO reference at batch level due to ambiguity.";
                    ExportCore.WriteLog(Constants.GE_LOG_PREFIX + message);
                    throw new SmartExportException(message);
                case Constants.Document:
                    output = currentIterationDCO == null ? getDCOValueForDocument(DCOTree) : 
                        getDCOValueForDocument(currentIterationDCO, DCOTree);
                    break;
                case Constants.Page:
                    output = currentIterationDCO == null ? getDCOValueForPage(DCOTree) : 
                        getDCOValueForPage(DCOTree, currentIterationDCO.ID);
                    break;
                case Constants.Field:
                    output = currentIterationDCO == null ? getDCOValueForField(DCOTree) : 
                        getDCOValueForField(currentIterationDCO, DCOTree);
                    break;
               


            }
            return output;
        }

        ///       <summary>
        ///       The method returns the value corresponding to the DCO field specified, from the current DCO for the field.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <returns>The value corresponding to the DCO field specified, from the current DCO.</returns>
        ///       </summary>
        public string getDCOValueForField(string DCOTree)
        {
            // DCO reference in the template file should adhere to a 4 part string [DCO].[document_type].[page_type].[field_name]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);
            TDCOLib.DCO page = CurrentDCO.Parent();
            string pageID = page.ID;
            TDCOLib.DCO document = page.Parent();
            string documentID = document.ID; 
            string output = "";
            try
            {
                // this is to pick up the field from the right page type
                if(dcoArray[3]== CurrentDCO.ID && dcoArray[1] == document.Type && dcoArray[2] == page.Type)
                {
                    output = CurrentDCO.Text;
                }
                else
                {
                    ExportCore.WriteLog(Constants.LOG_PREFIX + " Current field "+ CurrentDCO.ID +
                        " is  different from required field which is" + DCOTree);
                }
            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteLog(Constants.LOG_PREFIX + " Unable to find DCO reference for the field : " + DCOTree);
                ExportCore.WriteLog(Constants.LOG_PREFIX + " Error while reading DCO reference: " + exp.ToString());
            }

            return output;
        }

        
        ///       <summary>
        ///       The method returns the value corresponding to the field specified from the current DCO.
        ///       <param name="currentIterationDCO">It is the DCO which is currently iterated in for loop.</param>
        ///       <returns>The value corresponding to the DCO expression specified from the current DCO.</returns>
        ///       </summary>
        public string getDCOValueForField(TDCOLib.IDCO currentIterationDCO, string DCOTree)
        {
 
            // DCO reference in the template file should adhere to a 4 part string [DCO.<doc_type>.<page_type>.<field_name>]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);

            string output = "";
            try
            {
                TDCOLib.DCO page = DCO.FindChild(currentIterationDCO.Parent().ID);
                if(dcoArray[3] ==currentIterationDCO.ID)
                    output = page.FindChild(currentIterationDCO.ID).Text;
                else
                    ExportCore.WriteLog(Constants.GE_LOG_PREFIX + " Looking for field  " +DCOTree+ ", where as the current field is"+ currentIterationDCO.ID);

            }
            catch (Exception exp)
            {
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + " Unable to find field reference for the page with ID: " + currentIterationDCO.Parent().ID);
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + " Error while reading field reference: " + exp.ToString());
            }

            return output;
        }

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified from the current DCO.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <returns>The value corresponding to the DCO expression specified from the current DCO.</returns>
        ///       </summary>
        public string getDCOValueForPage(string DCOTree )
        {
            // DCO reference in the template file should adhere to a 4 part string [DCO.<doc_type>.<page_type>.<field_name>]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);
                
            string output = "";
            try
            {
                 
                if (dcoArray[1] == CurrentDCO.Parent().Type && dcoArray[2] == CurrentDCO.Type)
                {
                    output =CurrentDCO.FindChild(dcoArray[3]).Text;
                }
                else
                {
                    ExportCore.WriteLog(Constants.LOG_PREFIX + " The expression   " + DCOTree+ " is not valid for page  " + CurrentDCO.ID);
                }
                
            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + " Unable to find DCO reference for the page with ID: " + CurrentDCO.ID);
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + " Error while reading DCO reference: " + exp.ToString());
            }

            return output;
        }

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified from the current DCO.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <param name="pageID">Page ID from where the value needs to be extracted</param>
        ///       <returns>The value corresponding to the DCO expression specified from the current DCO.</returns>
        ///       </summary>
        public string getDCOValueForPage(string DCOTree, string pageID)
        {
            // DCO reference in the template file should adhere to a 4 part string [DCO.<doc_type>.<page_type>.<field_name>]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);

            string output = "";
            try
            {
                TDCOLib.DCO page = DCO.FindChild(pageID);
                if (dcoArray[1] == page.Parent().Type && dcoArray[2] == page.Type)
                {
                    output = page.FindChild(dcoArray[3]).Text;
                }
                else
                {
                    ExportCore.WriteLog(Constants.LOG_PREFIX + " The expression   " + DCOTree + " is not valid for page  " + pageID);
                }

            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + " Unable to find DCO reference for the page with ID: " + pageID);
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + " Error while reading DCO reference: " + exp.ToString());
            }

            return output;
        }

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified from the current DCO.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <returns>The value corresponding to the DCO expression specified from the current DCO.</returns>
        ///       </summary>
        public string getDCOValueForDocument(string DCOTree )
        {

            // DCO reference in the template file should adhere to a 4 part string [DCO.<doc_type>.<page_type>.<field_name>]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);

            // get the value of the DCO reference using the page ID and the field name
            string output = "";

      
            try
            {
                if(dcoArray[1] == CurrentDCO.Type)
                {
                    string pageID = getPageIDOfTypeInDocument(DCOTree, CurrentDCO.ID, dcoArray[2]);
                    output = CurrentDCO.FindChild(pageID).FindChild(dcoArray[3]).Text;
                }
                else
                {
                    ExportCore.WriteLog(Constants.LOG_PREFIX + " The expression   " + DCOTree + " is not valid for document  " + CurrentDCO.ID);

                }

            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteLog(Constants.LOG_PREFIX + " Unable to find DCO reference for the document with ID: " + CurrentDCO.ID);
                ExportCore.WriteLog(Constants.LOG_PREFIX + " Error while reading DCO reference: " + exp.ToString());
            }

            return output;
        }

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified from the current DCO.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <param name="documentID">ID of the document from where the value needs to be extracted</param>
        ///       <returns>The value corresponding to the DCO expression specified from the current DCO.</returns>
        ///       </summary>
        public string getDCOValueForDocument(TDCOLib.IDCO currentIterationDCO, string DCOTree)
        {

            // DCO reference in the template file should adhere to a 4 part string [DCO.<doc_type>.<page_type>.<field_name>]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);

            // get the value of the DCO reference using the page ID and the field name
            string output = "";


            try
            {
                TDCOLib.DCO document = DCO.FindChild(currentIterationDCO.ID);
                if (dcoArray[1] == document.Type)
                {
                    string pageID = getPageIDOfTypeInDocument(DCOTree, currentIterationDCO.ID, dcoArray[2]);
                    output = currentIterationDCO.FindChild(pageID).FindChild(dcoArray[3]).Text;
                }
                else
                {
                    ExportCore.WriteLog(Constants.LOG_PREFIX + " The expression   " + DCOTree + " is not valid for document  " + currentIterationDCO.ID);

                }

            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteLog(Constants.LOG_PREFIX + " Unable to find DCO reference for the document with ID: " + currentIterationDCO.ID);
                ExportCore.WriteLog(Constants.LOG_PREFIX + " Error while reading DCO reference: " + exp.ToString());
            }

            return output;
        }


        ///       <summary>
        ///       The method returns the page ID of the specified page typs in the specified document.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <param name="DocumentID">Docuemnt ID</param>
        ///       <param name="pageType">Page tyep</param>
        ///       <returns>The page ID of the specified page typs in the specified document.</returns>
        ///       </summary>
        public string getPageIDOfTypeInDocument(string DCOTree, string documentID, string pageType)
        {

            string pageID="";
            TDCOLib.IDCO currentIterationDCO = null; 
           if(!Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO).Equals(Constants.EMPTYSTRING)){
               currentIterationDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO);                           
            } 

            List<string>  pageIDs = new List<string>();
            int noOfChildren = currentIterationDCO == null ? CurrentDCO.NumOfChildren(): currentIterationDCO.NumOfChildren();
            for(int i = 0; i < noOfChildren; i++)
            {
                TDCOLib.DCO child = currentIterationDCO == null ? CurrentDCO.GetChild(i) : child = currentIterationDCO.GetChild(i);  ;
                //If the call is from ForEach, this will be having a currentIterationDCO object.
            
                if (child.Type == pageType)
                {
                    pageID = child.ID;
                    pageIDs.Add(child.ID);
                }
                if (1 < pageIDs.Count)
                {
                    string message = " There is more than one page of type " + pageType + " in the document " + documentID;
                    ExportCore.WriteLog(Constants.GE_LOG_PREFIX + message);
                    throw new SmartExportException(message);
                }
            }
           
            if (0 == pageIDs.Count)
            {
                string message = " There is no page of type " + pageType + " in the document " + documentID;
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + message);
            }

            return pageID;
            
        }

        public string getPageType()
        {

            string pageType = "";
            TDCOLib.IDCO currentIterationDCO = null;
            int objectType = CurrentDCO.ObjectType();

            //If the call is from ForEach, this will be having a currentIterationDCO object.
            if(!Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO).Equals(Constants.EMPTYSTRING)){
                currentIterationDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO);
                objectType = currentIterationDCO.ObjectType();
            }  

           if (Constants.Page == objectType)
            {
                pageType = currentIterationDCO == null ? CurrentDCO.Type : currentIterationDCO.Type;
            }
            else if (Constants.Field == objectType)
            {
                pageType = currentIterationDCO == null ? CurrentDCO.Parent().Type : currentIterationDCO.Parent().Type;
            }
            else
            {
                string message = "  Page Type can be determined at   page / field level only. " + CurrentDCO.ID
                    + " is of type " + CurrentDCO.Type + ".";
                if(currentIterationDCO != null){
                  message =  "  Page Type can be determined at   page / field level only. " + currentIterationDCO.ID
                    + " is of type " + currentIterationDCO.Type + ".";
                    }
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + message);
                throw new SmartExportException(message);
            }

            return pageType;
        }

        public string getDocumentType()
        {
            string docType = "";
            TDCOLib.IDCO currentIterationDCO = null;
            int objectType = CurrentDCO.ObjectType();

            //If the call is from ForEach, this will be having a currentIterationDCO object.
            if(!Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO).Equals(Constants.EMPTYSTRING)){
                currentIterationDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO);
                objectType = currentIterationDCO.ObjectType();
            }  

            if (Constants.Document == objectType)
            {
                docType = currentIterationDCO == null ? CurrentDCO.Type : currentIterationDCO.Type;
            }
            else if (Constants.Page == objectType)
            {
                docType = currentIterationDCO == null ? CurrentDCO.Parent().Type : currentIterationDCO.Parent().Type;
            }
            else if (Constants.Field == objectType)
            {
                docType = currentIterationDCO == null ? CurrentDCO.Parent().Parent().Type : currentIterationDCO.Parent().Parent().Type;
            }
            else 
            {
                string message = "  Document Type can be determined at document/ page / field level only. " + CurrentDCO.ID 
                    + " is of type "+ CurrentDCO.Type + ".";
                if(currentIterationDCO != null){
                message = "  Document Type can be determined at document/ page / field level only. " + currentIterationDCO.ID 
                    + " is of type "+ currentIterationDCO.Type + ".";
                    }
               
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + message);
                throw new SmartExportException(message);
            }

            return docType;
        }

    }
}



