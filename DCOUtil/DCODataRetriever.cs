using System;
using System.Collections.Generic;
using System.Xml;
using SmartExportTemplates.Utils;
using System.Diagnostics;
using SmartExportTemplates.TemplateCore;
using TDCOLib;

namespace SmartExportTemplates.DCOUtil
{
    class DCODataRetriever
    {
        private SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        protected TDCOLib.IDCO CurrentDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_CURRENT_DCO);
        protected TDCOLib.IDCO DCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_DCO);
        

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified, from the current DCO.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[document_type].[page_type].[field_name]</param>
        ///       <returns>The value corresponding to the DCO expression specified, from the current DCO.</returns>
        ///       </summary>
        public string getDCOValue(string DCOTree)        
        {
            Stopwatch sw = Stopwatch.StartNew();

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
                    ExportCore.WriteLog(message);
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
            ExportCore.WriteDebugLog(" getDCOValue(" + DCOTree + ") completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();
            
            return output;
        }

        ///       <summary>
        ///       Fetches the column value for a row of a table.
        ///       <param name="columnName">Column name</param>
        ///       <param name="pageID">Page ID from where the value needs to be extracted</param>
        ///       <returns>The value of the column for the current row of a table.</returns>
        public string getColumnValueForRow(string columnName)
        {
            string columnValue = "";
            TDCOLib.IDCO row = null;
            Stopwatch sw = Stopwatch.StartNew();

            // the current iternation DCO would point to a row of a table 
            if (!Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO).Equals(Constants.EMPTYSTRING))
            {
                row = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO);
                for (int k = 0; k < row.NumOfChildren(); k++)
                {
                    TDCOLib.DCO column = row.GetChild(k);
                    if (column.ObjectType() == Constants.Field && columnName == column.ID)
                    {
                        ExportCore.WriteLog(columnName + " = " + column.Text);
                        columnValue = column.Text;
                        break;
                    }
                }
            }
            else
            {
                string message = "  Error occured while fetching value for column " + columnName;                
                ExportCore.WriteLog(message);
                throw new SmartExportException(message);
            }
            ExportCore.WriteDebugLog(" getColumnValueForRow(" + columnName + ") completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();
            return columnValue;

        }

        ///       <summary>
        ///      Checks if a page contains a table.
        ///       <param name="page">DCO objects the corresponds to the current page that is being processed.</param>
        ///       <param name="tableName">Name of the table of interest</param>
        ///       <returns>True if a page contains a table.</returns>
        public bool doesPageContainsTable(IDCO page, string tableName)
        {
            Stopwatch sw = Stopwatch.StartNew();

            bool pageHasTable = false;
            for(int i=0;i<page.NumOfChildren();i++)
            {
                IDCO field = page.GetChild(i);
                pageHasTable = isObjectTable(field) && (field.ID == tableName);
                if (pageHasTable)
                    break;
            }
            ExportCore.WriteDebugLog(" doesPageContainsTable(" + page + ","+tableName+")" +
                " completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();
            return pageHasTable;
        }

        ///       <summary>
        ///       Returns the sepcified table DCO object if it is present in the page.
        ///       <param name="page">DCO objects the corresponds to the current page that is being processed.</param>
        ///       <param name="tableName">Name of the table of interest</param>
        ///       <returns>The sepcified table DCO object if it is present in the page</returns>
        public IDCO getTableForPage(IDCO page, string tableName)
        {
            Stopwatch sw = Stopwatch.StartNew();

            IDCO table = null;
            for (int i = 0; i < page.NumOfChildren(); i++)
            {
                IDCO field = page.GetChild(i);
                if (isObjectTable(field) && (field.ID == tableName))
                {
                    table = field;
                    break;
                }
            }
            ExportCore.WriteDebugLog(" getTableForPage(" + page + "," + tableName + ")" +
               " completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();
            return table;
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
                if((dcoArray[3]== CurrentDCO.ID 
                    && (dcoArray[1] == document.Type || page.Parent().ObjectType() == Constants.Batch) 
                    && dcoArray[2] == page.Type))
                {
                    output = CurrentDCO.Text;
                }              
                else
                {
                    ExportCore.WriteLog(" Current field "+ CurrentDCO.ID +
                        " is  different from required field which is" + DCOTree);
                }
            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteErrorLog(" Unable to find DCO reference for the field : " + DCOTree);
                ExportCore.WriteErrorLog(" Error while reading DCO reference: " + exp.ToString());
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
                if (dcoArray.Length == 2 && DCOTree == "field.name")
                {
                    output = currentIterationDCO.ID ;
                }
                else if (dcoArray[3] == currentIterationDCO.ID)           
                    output = page.FindChild(currentIterationDCO.ID).Text;
                else
                    ExportCore.WriteLog(" Looking for field  " +DCOTree+ ", where as the current field is"+ currentIterationDCO.ID);

            }
            catch (Exception exp)
            {
                ExportCore.WriteErrorLog(" Unable to find field reference for the page with ID: " + currentIterationDCO.Parent().ID);
                ExportCore.WriteErrorLog(" Error while reading field reference: " + exp.ToString());
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
                 // Validate DCOTree expression against the current DCO
                 // match page type and parent type of page
                if (dcoArray[2] == CurrentDCO.Type &&
                    (dcoArray[1] == CurrentDCO.Parent().Type || CurrentDCO.Parent().ObjectType() == Constants.Batch))
                {                   
                    output = CurrentDCO.FindChild(dcoArray[3]).Text;
                    ExportCore.WriteDebugLog(" Value of  expression   " + DCOTree + " is   " + output);
                }
                else
                {
                    ExportCore.WriteLog(" The expression   " + DCOTree+ " is not valid for page  " + CurrentDCO.ID);
                }
                
            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteLog(" Unable to find DCO reference for the page with ID: " + CurrentDCO.ID);
                ExportCore.WriteLog(" Error while reading DCO reference: " + exp.ToString());
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
                if (dcoArray.Length == 2 && DCOTree == "page.name")
                {
                    output = page.ID + " - " + page.Type;
                }
                // Validate DCOTree expression against the current DCO
                // match page type and parent type of page
                else if ((dcoArray[1] == page.Parent().Type  || page.Parent().ObjectType() == Constants.Batch)
                    && dcoArray[2] == page.Type)
                {
                    output = page.FindChild(dcoArray[3]).Text;
                }
                else
                {
                    ExportCore.WriteLog(" The expression   " + DCOTree + " is not valid for page  " + pageID);
                }

            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteErrorLog(" Unable to find DCO reference for the page with ID: " + pageID);
                ExportCore.WriteErrorLog(" Error while reading DCO reference: " + exp.ToString());
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
                    ExportCore.WriteLog(" The expression   " + DCOTree + " is not valid for document  " + CurrentDCO.ID);
                }
            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteErrorLog(" Unable to find DCO reference for the document with ID: " + CurrentDCO.ID);
                ExportCore.WriteErrorLog(" Error while reading DCO reference: " + exp.ToString());
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
                if (dcoArray.Length == 2 && DCOTree == "document.name")
                {
                    output = currentIterationDCO.ID + " - " + currentIterationDCO.Type;
                }
                else if (dcoArray[1] == document.Type)
                {
                    string pageID = getPageIDOfTypeInDocument(DCOTree, currentIterationDCO.ID, dcoArray[2]);
                    output = currentIterationDCO.FindChild(pageID).FindChild(dcoArray[3]).Text;
                }
                else
                {
                    ExportCore.WriteLog(" The expression   " + DCOTree + " is not valid for document  " + currentIterationDCO.ID);
                }

            }
            catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                ExportCore.WriteErrorLog(" Unable to find DCO reference for the document with ID: " + currentIterationDCO.ID);
                ExportCore.WriteErrorLog(" Error while reading DCO reference: " + exp.ToString());
            }

            return output;
        }


        ///       <summary>
        ///       The method returns the page ID of the specified page type in the specified document.
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
                    ExportCore.WriteLog(message);
                    throw new SmartExportException(message);
                }
            }
           
            if (0 == pageIDs.Count)
            {
                string message = " There is no page of type " + pageType + " in the document " + documentID;
                ExportCore.WriteLog(message);
            }

            return pageID;
            
        }

        ///       <summary>
        ///       Returns the page type.
        ///       <returns>The page type.</returns>
        ///       </summary>
        public string getPageType()
        {
            Stopwatch sw = Stopwatch.StartNew();

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
                ExportCore.WriteLog(message);
                throw new SmartExportException(message);
            }
            ExportCore.WriteDebugLog(" getPageType() completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();

            return pageType;
        }

        ///       <summary>
        ///       Returns the document type.
        ///       <returns>The document type</returns>
        ///       </summary>
        public string getDocumentType()
        {
            Stopwatch sw = Stopwatch.StartNew();

            string docType = "";
            TDCOLib.IDCO currentIterationDCO = null;
            int objectType = CurrentDCO.ObjectType();

            //If the call is from ForEach, this will be having a currentIterationDCO object.
            if(!Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO).Equals(Constants.EMPTYSTRING)){
                currentIterationDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO);
                objectType = currentIterationDCO.ObjectType();
                //anything getter related to currentIterationDCO object must be added here or after null check , 
                //if not there is a risk of null pointer exception.
                ExportCore.WriteDebugLog(" Current iterating DCO: " + currentIterationDCO.Type);
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
               
                ExportCore.WriteErrorLog(message);
                throw new SmartExportException(message);
            }
            ExportCore.WriteDebugLog(" getDocumentType() completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();

            return docType;
        }

        ///       <summary>
        ///       Returns the table type.
        ///       <returns>The table type</returns>
        ///       </summary>
        public string getTableType()
        {
            Stopwatch sw = Stopwatch.StartNew();

            string tableType = "";
            TDCOLib.IDCO DCO = CurrentDCO;
            if (!Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO).Equals(Constants.EMPTYSTRING))
            {
                DCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.forLoopString.CURRENTITERATIONDCO);

            }
            int objectType = DCO.ObjectType();

            if (Constants.Field == objectType)
            {
                if (isObjectTable(DCO))
                    tableType = DCO.Type;
                else
                {
                    string message = CurrentDCO.ID + " is not a table. It is of type " + CurrentDCO.Type + ".";
                    ExportCore.WriteLog(message);
                }
            }
            else
            {
                string message = "  Table Type can be determined at   field level only. " + CurrentDCO.ID
                    + " is of type " + CurrentDCO.Type + ".";

                ExportCore.WriteLog(message);
                throw new SmartExportException(message);
            }
            ExportCore.WriteDebugLog(" getTableType() completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();

            return tableType;
        }

        ///       <summary>
        ///       Checks if the DCO object is a table.
        ///       <param name="table">DCO object</param>
        ///       <returns>True if the DCO object is a table</returns>
        ///       </summary>
        public bool isObjectTable(TDCOLib.IDCO table)
        {
            bool isTable = false;

            for (int j = 0; j < table.NumOfChildren(); j++)
            {
                TDCOLib.DCO row = table.GetChild(j);
                if (row.ObjectType() == Constants.Field)
                {
                    for (int k = 0; k < row.NumOfChildren(); k++)
                    {
                        TDCOLib.DCO column = row.GetChild(k);

                        if (column.ObjectType() == Constants.Field)
                        {
                            isTable = true;
                            break;
                        }
                    }
                }
            }
            return isTable;
        }

    }
}



