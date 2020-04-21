using System;
using System.Collections.Generic;
using System.Xml;
using SmartExportTemplates.Utils;
using System.Diagnostics;
using SmartExportTemplates.TemplateCore;
using TDCOLib;

namespace SmartExportTemplates.DCOUtil
{
    class DCODataRetrieverWithoutDoc : DCODataRetriever
    {
        private SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        protected TDCOLib.IDCO CurrentDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_CURRENT_DCO);
        protected TDCOLib.IDCO DCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_DCO);
        private dcSmart.SmartNav SmartNav = (dcSmart.SmartNav)Globals.Instance.GetData(Constants.GE_SMART_NAV);

        public void createFilePageMap()
        {
            Dictionary<string, List<string>> filePageMap = new Dictionary<string, List<string>>();
            if (CurrentDCO.ObjectType() == Constants.Batch)
            {
                for (int i = 0; i < CurrentDCO.NumOfChildren(); i++)
                {
                    TDCOLib.IDCO childDCO = CurrentDCO.GetChild(i);
                    if (childDCO.ObjectType() == Constants.Page)
                    {
                        SmartNav.SetRRCurrentDCO(childDCO);
                        string imageName = SmartNav.MetaWord(Constants.SMARTP_AT + "P.ScanSrcPath");
                        List<string> pages = null;
                        if (filePageMap.ContainsKey(imageName))
                        {
                            pages = filePageMap[imageName];
                        }
                        else
                        {
                            pages = new List<string>();
                        }
                        pages.Add(childDCO.ID);
                        filePageMap[imageName] = pages;
                        ExportCore.WriteInfoLog(" file page map   " + imageName + " : " + childDCO.ID + "; size : " + pages.Count);
                    }
                }
                Globals.Instance.SetData(Constants.FILE_PAGE_MAP, filePageMap);
                SmartNav.SetRRCurrentDCO(CurrentDCO);
            }
            else
            {
                throw new SmartExportException("Unable to create File-Page map, since the associated level is not  Batch.");
            }
        }

        public string getDCOValueForFile(string DCOTree)
        {
            string filename = (string)Globals.Instance.GetData(Constants.forLoopString.CURRENTFILE);
            Dictionary<string, List<string>> filePageMap = (Dictionary<string, List<string>>)Globals.Instance.GetData(Constants.FILE_PAGE_MAP);
            List<string> pages = (List<string>)filePageMap[filename];
            string output = "";
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep);

            if (CurrentDCO.ObjectType() != Constants.Batch)
                throw new SmartExportException("getDCOValueForFile(" + DCOTree + ") can be used at batch level only. ");

            foreach (string pageID in pages)
            {
                TDCOLib.DCO page = CurrentDCO.FindChild(pageID);
                // Validate DCOTree expression against the current DCO
                // match page type and parent type of page
                if (page.Parent().ObjectType() == Constants.Batch
                   && dcoArray[1] == page.Type)
                {
                    output += page.FindChild(dcoArray[2]).Text;
                    if (!string.IsNullOrEmpty(output))
                        break;
                }
                else
                {
                    ExportCore.WriteLog(" The expression   " + DCOTree + " is not valid for page  " + pageID);
                }
            }
            return output;
        }

        ///       <summary>
        ///       The method returns the value corresponding to the DCO field specified, from the current DCO for the field.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[page_type].[field_name]</param>
        ///       <returns>The value corresponding to the DCO field specified, from the current DCO.</returns>
        ///       </summary>
        public override string getDCOValueForField(string DCOTree)
        {
            // DCO reference in the template file should adhere to a 4 part string [DCO].[page_type].[field_name]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);
            TDCOLib.DCO page = CurrentDCO.Parent();
            string pageID = page.ID;
            
            string output = "";
            try
            {
                // this is to pick up the field from the right page type
                if (dcoArray[2] == CurrentDCO.ID && dcoArray[1] == page.Type)
                {
                    output = CurrentDCO.Text;
                }
                else
                {
                    ExportCore.WriteLog(" Current field " + CurrentDCO.ID +
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
        ///       The method returns the value corresponding to the DCO expression specified, from the current DCO.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[page_type].[field_name]</param>
        ///       <returns>The value corresponding to the DCO expression specified, from the current DCO.</returns>
        ///       </summary>
        public override string getDCOValue(string DCOTree)
        {
            Stopwatch sw = Stopwatch.StartNew();

            string output = "";
            int objectType = CurrentDCO.ObjectType();
            
            if (objectType == Constants.Page)
            {
                output = getDCOValueForPage(DCOTree);
            }
            else if(objectType == Constants.Field)
            {
                output = getDCOValueForField(DCOTree);
            }
            //If the call is from ForEach, this will be having a currentIterationDCO object.
            else if (!Globals.Instance.GetData(Constants.forLoopString.CURRENTFILE).Equals(Constants.EMPTYSTRING) && objectType == Constants.Batch)
            {
                output = getDCOValueForFile(DCOTree);
            } 
            else
            {
                throw new SmartExportException("The value for expression " + DCOTree + " could not be evaluated.");
            }
           
            ExportCore.WriteDebugLog(" getDCOValue(" + DCOTree + ") completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();

            return output;
        }

        ///       <summary>
        ///       The method returns the value corresponding to the DCO expression specified from the current DCO.
        ///       <param name="DCOTree">DCO Expression in the format [DCO].[page_type].[field_name]</param>
        ///       <returns>The value corresponding to the DCO expression specified from the current DCO.</returns>
        ///       </summary>
        public override string getDCOValueForPage(string DCOTree)
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
                if (dcoArray[1] == CurrentDCO.Type )
                {
                    output = CurrentDCO.FindChild(dcoArray[2]).Text;
                    ExportCore.WriteDebugLog(" Value of  expression   " + DCOTree + " is   " + output);
                }
                else
                {
                    ExportCore.WriteLog(" The expression   " + DCOTree + " is not valid for page  " + CurrentDCO.ID);
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
        ///       Returns the page type.
        ///       <returns>The page type.</returns>
        ///       </summary>
        public override string getPageTypesInFile()
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (CurrentDCO.ObjectType() != Constants.Batch)
            {
                throw new SmartExportException("All Page Types In File can be determined at batch level only.");
            }

            List<string> pageTypes = new List<string>();
            string fileName = (string)Globals.Instance.GetData(Constants.forLoopString.CURRENTFILE);
            if (!fileName.Equals(Constants.EMPTYSTRING))
            {
                Dictionary<string, List<string>> filePageMap =
                    (Dictionary<string, List<string>>)Globals.Instance.GetData(Constants.FILE_PAGE_MAP);
                List<string> pages = filePageMap[fileName];
                foreach (string pageID in pages)
                {
                    ExportCore.WriteDebugLog("Adding page type " + CurrentDCO.FindChild(pageID).Type);
                    pageTypes.Add(CurrentDCO.FindChild(pageID).Type);
                }
            }
            else
            {
                throw new SmartExportException("Cannot determine page types when file name is unknown.");
            }

            ExportCore.WriteDebugLog(" getPageTypesInFile() completed in " + sw.ElapsedMilliseconds + " ms.");
            sw.Stop();

            return string.Join(",", pageTypes);
        }

        public List<IDCO> getTablesForFile(string fileName, string tableName)
        {
            List<IDCO> tables = new List<IDCO>();
            Dictionary<string, List<string>> filePageMap = (Dictionary<string, List<string>>)Globals.Instance.GetData(Constants.FILE_PAGE_MAP);
            List<string> pages = filePageMap[fileName];
            if(CurrentDCO.ObjectType()==Constants.Batch)
            {
                foreach(string pageID in pages)
                {
                    DCO page = CurrentDCO.FindChild(pageID);
                    tables.Add(getTableForPage(page, tableName));
                    
                }
            }
            return tables;
        }

    }
}



