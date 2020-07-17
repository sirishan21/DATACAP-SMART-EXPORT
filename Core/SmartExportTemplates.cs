//
// Licensed Materials - Property of IBM
//
// 5725-C15
// © Copyright IBM Corp. 1994, 2019 All Rights Reserved
// US Government Users Restricted Rights - Use, duplication or
// disclosure restricted by GSA ADP Schedule Contract with IBM Corp.
//
// SmartExportTemplates are used to transform DataCap output into formated files based on the input template.  

using System;
using System.Runtime.InteropServices;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//Project namespaces
using SmartExportTemplates.DCOUtil;
using SmartExportTemplates.TemplateCore;
using SmartExportTemplates.Utils;
using SmartExportTemplates.Core;

namespace SmartExportTemplates
{
    public class SmartExport
    {
        #region ExpectedByRRS
        /// <summary/>
        ~SmartExport()
        {
            DatacapRRCleanupTime = true;
        }

        /// <summary>
        /// Cleanup: This property is set right before the object is released by RRS
        /// The Dispose method is not called by RRS.
        /// </summary>
        public bool DatacapRRCleanupTime
        {
            set
            {
                if (value)
                {
                    CleanUp();
                    CurrentDCO = null;
                    DCO = null;
                    RRLog = null;
                    RRState = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }

        protected PILOTCTRLLib.IBPilotCtrl BatchPilot = null;
        public PILOTCTRLLib.IBPilotCtrl DatacapRRBatchPilot { set { this.BatchPilot = value; GC.Collect(); GC.WaitForPendingFinalizers(); } get { return this.BatchPilot; } }

        protected TDCOLib.IDCO DCO = null;
        /// <summary/>
        public TDCOLib.IDCO DatacapRRDCO
        {
            get { return this.DCO; }
            set
            {
                DCO = value;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        protected dcrroLib.IRRState RRState = null;
        /// <summary/>
        public dcrroLib.IRRState DatacapRRState
        {
            get { return this.RRState; }
            set
            {
                RRState = value;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public TDCOLib.IDCO CurrentDCO = null;

        /// <summary/>
        public TDCOLib.IDCO DatacapRRCurrentDCO
        {
            get { return this.CurrentDCO; }
            set
            {
                CurrentDCO = value;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public dclogXLib.IDCLog RRLog = null;
        /// <summary/>
        public dclogXLib.IDCLog DatacapRRLog
        {
            get { return this.RRLog; }
            set
            {
                RRLog = value;
                LogAssemblyVersion();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        #endregion

        #region CommonActions

        void OutputToLog(int nLevel, string strMessage)
        {
            if (null == RRLog)
                return;
            RRLog.WriteEx(nLevel, strMessage);
        }
        //to log errors
        public void WriteErrorLog(string sMessage)
        {
            OutputToLog(1, Constants.GE_ERROR_LOG_PREFIX + sMessage);
        }
        //to log info 
        public void WriteInfoLog(string sMessage)
        {
            OutputToLog(2, Constants.GE_INFO_LOG_PREFIX + sMessage);
        }
        //to log debug 
        public void WriteDebugLog(string sMessage)
        {
            OutputToLog(3, Constants.GE_DEBUG_LOG_PREFIX +sMessage);
        }
        //to log full/All 
        public void WriteLog(string sMessage)
        {
            OutputToLog(5, Constants.GE_ALL_LOG_PREFIX + sMessage);
        }

        //this object is to access smart parameters.
        dcSmart.SmartNav smartNav = null;

        private bool versionWasLogged = false;

        // Log the version of the library that was running to help with diagnosis.
        // Hooked this method to be called after the log object is assigned.  Also put in
        // a check that this action runs only once, just in case it gets called multiple times.
        protected bool LogAssemblyVersion()
        {
            try
            {
                if (versionWasLogged == false)
                {
                    FileVersionInfo fv = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                    WriteLog(Assembly.GetExecutingAssembly().Location +
                             ". AssemblyVersion: " + Assembly.GetExecutingAssembly().GetName().Version.ToString() +
                             ". AssemblyFileVersion: " + fv.FileVersion.ToString() + ".");
                    versionWasLogged = true;
                }
            }
            catch (Exception ex)
            {
                WriteLog("Version logging exception: " + ex.Message);
            }

            // We can always return true.  If getting the version fails, we can try to continue anyway.
            return true;
        }

        #endregion


        // implementation of the Dispose method to release managed resources
        // There is no guarentee that dispose will be called.  Also note, class distructors are also not called.  CleanupTime is called by RRS.        
        public void Dispose()
        {
            CleanUp();
        }

        /// <summary>
        /// Everthing that should be cleaned up on exit
        /// It is recommended to avoid logging during cleanup.
        /// </summary>
        protected void CleanUp()
        {
            try
            {
                // Cleanup and release any allocated objects here. This will be called before the DLL is released.
            }
            catch { } // Ignore any errors.
        }

        struct Level
        {
            internal const int Batch = 0;
            internal const int Document = 1;
            internal const int Page = 2;
            internal const int Field = 3;
        }

        struct Status
        {
            internal const int Hidden = -1;
            internal const int OK = 0;
            internal const int Fail = 1;
            internal const int Over = 3;
            internal const int RescanPage = 70;
            internal const int VerificationFailed = 71;
            internal const int PageOnHold = 72;
            internal const int PageOverridden = 73;
            internal const int NoData = 74;
            internal const int DeletedPage = 75;
            internal const int ExportComplete = 76;
            internal const int DeleteApproved = 77;
            internal const int ReviewPage = 79;
            internal const int DeletedDoc = 128;
        }


        // NodeType
        public struct NodeType
        {
            internal const int Data = 0;
            internal const int If = 1;
            internal const int ForEach = 2;
            internal const int ForEachRows = 3;
            internal const int Header = 4;
            internal const int Invalid = 2147482647;
        }

        //Global variables
        // document ID reference used by the child methods during processing.
        string DocumentID = null;

        // List of valid DCO expressions that can be used within the template
        List<string> DCOPatterns = new List<string>();

        //Map containing file names used when output is written to single file.
        //key - outputfilename: value - outputfile
        Dictionary<string, string> singleOutputFileNameMap = new Dictionary<string, string>();

        //util class to write output file
        private SmartExportUtil exportUtil = new SmartExportUtil();

        public SmartExportUtil getExportUtil {
            get
            {
                return this.exportUtil;
            }
        }

        private void SetGlobals()
        {
            // Set the global references into thread local for use by the different modules
            // No need to worry about threads here (given the way CurrentDCO works) and we can use a Singleton Globals class           
            Globals.Instance.SetData(Constants.GE_CURRENT_DCO, CurrentDCO);
            Globals.Instance.SetData(Constants.GE_DCO, DCO);
            Globals.Instance.SetData(Constants.GE_DCO_REF_PATTERN, Constants.DCO_REF_PATTERN);
            Globals.Instance.SetData(Constants.GE_EXPORT_CORE, this);
            string batchXMLFile = this.BatchPilot.DCOFile;
            string batchDirPath = Path.GetDirectoryName(batchXMLFile);
            Globals.Instance.SetData(Constants.GE_BATCH_DIR_PATH, batchDirPath);
            Globals.Instance.SetData(Constants.CSV_HEADERS, "");
            Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, Constants.EMPTYSTRING);
            Globals.Instance.SetData(Constants.GE_SMART_NAV, smartNav);
        }

       

        public bool FormattedDataOutput(string TemplateFilePath)
        {
            bool returnValue = true;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                smartNav = new dcSmart.SmartNav(this);

                //set thread locals
                SetGlobals();
                //TODO: Validate template
                //Initialize the parser
                TemplateParser templateParser = new TemplateParser(TemplateFilePath);
                templateParser.Parse();
                
                exportUtil.setContext(templateParser);

                if (templateParser.AppendToFile() && !singleOutputFileNameMap.ContainsKey(templateParser.GetOutputFileName()))
                {
                    singleOutputFileNameMap.Add(templateParser.GetOutputFileName(), templateParser.GetOutputFileName() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff"));
                }
                string locale = templateParser.GetLocale();
                Globals.Instance.SetData(Constants.LOCALE, locale);
                bool projectHasDocument = doesProjectHaveDocument();
                Globals.Instance.SetData(Constants.PROJECT_HAS_DOC, projectHasDocument);
                if (projectHasDocument)
                {
                    ContentProcessorWithDoc ContentProcessor = new ContentProcessorWithDoc(templateParser);
                    DCOPatterns.AddRange(ContentProcessor.createDCOPatternList(getDCODefinitionFile()));
                    ValidateExpressions(TemplateFilePath,Constants.DCO_REF_PATTERN);
                    ContentProcessor.processNodes();
                }
                else
                {
                    ContentProcessorWithoutDoc ContentProcessor = new ContentProcessorWithoutDoc(templateParser);
                    DCOPatterns.AddRange(ContentProcessor.createDCOPatternList(getDCODefinitionFile()));
                    ValidateExpressions(TemplateFilePath, Constants.DCO_REF_PATTERN_NO_DOC);
                    ContentProcessor.processNodes();
                    

                }
                //if project has document
                //or when project doesn't have document and action is attached at page/field level
                //or when collate batch output flag is true when action is attached at batch level and project doesn't have 
                //document
                if (projectHasDocument
                    || (CurrentDCO.ObjectType() != Constants.Batch && !projectHasDocument)
                    || (CurrentDCO.ObjectType() == Constants.Batch && !projectHasDocument
                        && templateParser.CollateBatchOutput()))
                    exportUtil.writeToFile(singleOutputFileNameMap);

                WriteInfoLog(" Smart export WriteLog completed in " + sw.ElapsedMilliseconds+" ms.");

                sw.Stop();

            }
            catch (System.Exception exp)
            {
                returnValue = false;
                WriteErrorLog("Error while processing the template file: " + exp.Message);
                WriteErrorLog(exp.StackTrace);

            }

            // TODO: Catch the important exceptions here...
            return returnValue;
        }

        ///       <summary>
        ///       The method returns true if the project contains a document and false otherwise.
        ///       </summary>
        private bool doesProjectHaveDocument()
        {
            List<bool> docStatus = new List<bool>();
            bool projectHasDoc = false;
            // if there is a document type in the DCO hierarchy
            if (CurrentDCO.ObjectType() == Constants.Document
                || (CurrentDCO.ObjectType() == Constants.Page && CurrentDCO.Parent().ObjectType() == Constants.Document)
                || (CurrentDCO.ObjectType() == Constants.Field && CurrentDCO.Parent().ObjectType() == Constants.Page &&
                     CurrentDCO.Parent().Parent().ObjectType() == Constants.Document)
               )
                projectHasDoc = true;
            //check if the children of batch are not  document           
            else if(CurrentDCO.ObjectType() == Constants.Batch)
            {
                for(int i=0;i< CurrentDCO.NumOfChildren();i++)
                {
                    TDCOLib.IDCO childDCO = CurrentDCO.GetChild(i);
                    if(childDCO.ObjectType() != Constants.Document)
                    {
                        docStatus.Add(false);
                    }
                    else
                    {
                        docStatus.Add(true);
                    }
                }
                projectHasDoc = docStatus.Contains(true);
            }
            return projectHasDoc;
        }

        ///       <summary>
        ///       The method returns the path of DCO definition file.
        ///       </summary>
        private string getDCODefinitionFile()
        {
            string parentDirectory = Path.GetDirectoryName(this.BatchPilot.ProjectPath);
            return parentDirectory + Path.DirectorySeparatorChar 
                + Path.GetFileName(Path.GetDirectoryName(parentDirectory)) + ".xml";
        }

   

        ///       <summary>
        ///       The method checks if valid DCO references are used in the template file. If an invalid reference if found an exception is thrown.
        ///       <param name="TemplateFile">Fully qualified path of the template file.</param>
        ///       <param name="dcoPattern">DCO Pattern.</param>
        ///       </summary>
        private void ValidateExpressions(string TemplateFile, string dcoPattern)
        {

            byte[] bytes = System.IO.File.ReadAllBytes(TemplateFile);
            string text = System.Text.Encoding.UTF8.GetString(bytes);

            Regex rg = new Regex(dcoPattern);
            MatchCollection matchedPatterns = rg.Matches(text);

            List<string> wrongPatterns = new List<string>();

            foreach (Match Pattern in matchedPatterns)
            {
                string DCOTree = Pattern.Value;
                DCOTree = DCOTree.Replace("[", "").Replace("]", "").Replace("DCO.", "");
                if (!DCOPatterns.Contains(DCOTree))
                {
                    WriteLog("DCO reference is invalid. " + Pattern.Value);
                    wrongPatterns.Add(Pattern.Value);
                }
            }
            if (wrongPatterns.Count > 0)
            {
                TemplateParser templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);
                throw new System.ArgumentException("The following DCO references are invalid - " +
                    string.Join(",", wrongPatterns) + " and can be found at line numbers " +
                    templateParser.GetLineNumberForPatterns(wrongPatterns));

            }
        }

    }
}