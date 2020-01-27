using System;
using System.Xml;
using SmartExportTemplates.Utils;

namespace SmartExportTemplates.DCOUtil
{
    class DCODataRetriever
    {
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        protected TDCOLib.IDCO DCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_DCO);
        protected TDCOLib.IDCO CurrentDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_CURRENT_DCO);

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified, from the current DCO.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <returns>The value corresponding to the DCO expression specified, from the current DCO.</returns>
        ///       </summary>
        public string getDCOValue(string DCOTree)        
        {
            string output = "";
      
            int objectType = CurrentDCO.ObjectType();
            
            switch (objectType)
            {
                case Constants.Batch:
                    string message = "Unable to find DCO reference at batch level due to ambiguity.";
                    ExportCore.WriteLog(Constants.GE_LOG_PREFIX + message);
                    throw new SmartExportException(message);
                case Constants.Document:
                    output = getDCOValueForDocument(DCOTree, CurrentDCO.ID);
                    break;
                case Constants.Page:
                    output = getDCOValueForPage(DCOTree, CurrentDCO.ID);
                    break;
                case Constants.Field:
                    output = getDCOValueForField(DCOTree);
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
            string pageID = CurrentDCO.Parent().ID;
            string output = null;
            try
            {
                // this is to pick up the field from the right page type
                if(dcoArray[3]== CurrentDCO.ID && dcoArray[2] == getPageType(pageID))
                {
                    return getDCOValueForPage(DCOTree, CurrentDCO.Parent().ID);
                }
                else
                {
                    ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "Current field "+ CurrentDCO.ID +
                        " is not different from required field which is" + dcoArray[3]);
                }
            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "Unable to find DCO reference for the field with ID: " + dcoArray[3]);
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "Error while reading DCO reference: " + exp.ToString());
            }

            return output;
        }

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified from the current DCO for a given page.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <param name="pageID">Page ID</param>
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

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified from the current DCO for a given document.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <param name="DocumentID">Docuemnt ID</param>
        ///       <returns>The value corresponding to the DCO expression specified from the current DCO.</returns>
        ///       </summary>
        public string getDCOValueForDocument(string DCOTree, string DocumentID )
        {

            // DCO reference in the template file should adhere to a 4 part string [DCO.<doc_type>.<page_type>.<field_name>]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);

            // get the value of the DCO reference using the page ID and the field name
            string output = null;

      
            try
            {
                string pageID = getPageIDOfTypeInDocument(DCOTree, DocumentID, dcoArray[2]);
                output = CurrentDCO.FindChild(pageID).FindChild(dcoArray[3]).Text;
                
            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "Unable to find DCO reference for the document with ID: " + DocumentID);
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "Error while reading DCO reference: " + exp.ToString());
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
            XmlDocument batchXML = new XmlDocument();
            batchXML.Load((string)Globals.Instance.GetData(Constants.GE_BATCH_XML_FILE));
            XmlElement batchRoot = batchXML.DocumentElement;
            XmlNodeList dcoDocumentNodes = batchRoot.SelectNodes("./D"); //Document nodes
            int pageCount = 0;
            Boolean foundDoc = false;
            foreach (XmlNode dcoDocumentNode in dcoDocumentNodes)
            {
                string ID = ((XmlElement)dcoDocumentNode).GetAttribute("id");
                if (ID == documentID)
                {
                    foundDoc = true;
                       XmlNodeList pageList = dcoDocumentNode.SelectNodes("./P");
                    // Data to be processed is always within pages
                    if (pageList.Count == 0)
                    {
                        ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "No pages in document: " + ID);
                        break;
                    }
                    foreach (XmlNode pageNode in pageList)
                    {
                        string type = pageNode.SelectSingleNode("./V[@n='TYPE']").InnerText;
                        
                        if (pageType == type)
                        {
                            pageCount++;
                            pageID = ((XmlElement)pageNode).GetAttribute("id");
                           
                        }
                        if(pageCount > 1)
                        {
                            pageID = "";
                            string message = "More than 1 page of type " + pageType + "  found in document: " + ID +
                                "Expression cannot be evaluated due to ambiguity. " + DCOTree;
                            ExportCore.WriteLog(Constants.GE_LOG_PREFIX + message );
                            throw new SmartExportException(message);
                           
                        }

                    }
                    break;
                }
                
            }
            if (!foundDoc)
            {
                string message = "Document with ID " + documentID + " not  found.";
                ExportCore.WriteLog(Constants.GE_LOG_PREFIX + message);
                throw new SmartExportException(message);
            }
            return pageID;
            
        }


        ///       <summary>
        ///       The method returns the page type of the specified page.
        ///       <param name="pageID">Page ID</param>
        ///       <returns>The page ID of the specified page typs in the specified document.</returns>
        ///       </summary>
        public string getPageType(string pageID)
        {
            string pageType ="";

            XmlDocument batchXML = new XmlDocument();
            batchXML.Load((string)Globals.Instance.GetData(Constants.GE_BATCH_XML_FILE));
            XmlElement batchRoot = batchXML.DocumentElement;
            XmlNodeList dcoDocumentNodes = batchRoot.SelectNodes("./D"); //Document nodes
            foreach (XmlNode dcoDocumentNode in dcoDocumentNodes)
            {
                String DocumentID = ((XmlElement)dcoDocumentNode).GetAttribute("id");
                XmlNodeList pageList = dcoDocumentNode.SelectNodes("./P");
                // Data to be processed is always within pages
                if (pageList.Count == 0)
                {
                    ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "No data to proces. Processing skipped for document: " + DocumentID);
                    continue;
                }

                foreach (XmlNode pageNode in pageList)
                {
                    string ID = ((XmlElement)pageNode).GetAttribute("id");                    
                    if (pageID == ID)
                    {
                        pageType = pageNode.SelectSingleNode("./V[@n='TYPE']").InnerText;
                        break;
                    }
                }               
            }
            return pageType;
        }

        

    }
}



