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

        public void WriteLog(string sMessage)
        {
            OutputToLog(5, sMessage);
        }

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
            internal const int Invalid = 2147482647;
        }

        // Global constants
        readonly string LOG_PREFIX = "DBA-SmartExport - ";

        //Global variables
        // document ID reference used by the child methods during processing.
        string DocumentID = null;
   
        // List of valid DCO expressions that can be used within the template
        List<string> DCOPatterns = new List<string>();
       
        //Map containing file names used when output is written to single file.
        Dictionary<string,string> singleOutputFileNameMap = new Dictionary<string,string>();

        private void SetGlobals()
        {
            // Set the global references into thread local for use by the different modules
            // No need to worry about threads here (given the way CurrentDCO works) and we can use a Singleton Globals class
            if (CurrentDCO.ObjectType() == Constants.Page)
            {
                Globals.Instance.SetData(Constants.IMAGE_NAME, CurrentDCO.ImageName);
                dcSmart.SmartNav smartObj = new dcSmart.SmartNav(this);
                Globals.Instance.SetData(Constants.GE_SMART_NAV, smartObj);
            }
            Globals.Instance.SetData(Constants.GE_CURRENT_DCO, CurrentDCO);
            Globals.Instance.SetData(Constants.GE_DCO, DCO);
            Globals.Instance.SetData(Constants.GE_LOG_PREFIX, LOG_PREFIX);
            Globals.Instance.SetData(Constants.GE_DCO_REF_PATTERN, Constants.DCO_REF_PATTERN);
            Globals.Instance.SetData(Constants.GE_EXPORT_CORE, this); 
            string batchXMLFile = this.BatchPilot.DCOFile;
            string batchDirPath = Path.GetDirectoryName(batchXMLFile);
            Globals.Instance.SetData(Constants.GE_BATCH_DIR_PATH, batchDirPath);
            Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, Constants.EMPTYSTRING);
        }


        private void writeToFile(TemplateParser templateParser,List<string> OutputData)
        {
            if (OutputData.Count == 0)
            {
                WriteLog(LOG_PREFIX + "Empty content. Skipping writing to file: " + templateParser.GetOutputFileName());
                return;
            }
            // Write to output file
            string outputFileName = templateParser.GetOutputFileName() + "_" 
                                        + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff") + '.'  
                                        + templateParser.GetOutputFileExt();
            string outputFilePath = Path.Combine(templateParser.GetOutputDirectory(), 
                                        templateParser.AppendToFile() ? 
                                            singleOutputFileNameMap[templateParser.GetOutputFileName()] + "." + templateParser.GetOutputFileExt()
                                            : outputFileName);

            //if AppendToFile is false then everytime new file is given then it creates a new file.
            //if AppendToFile is true then everytime singleOutputFileName file is given then it appends to the same file.
            using (StreamWriter outputFile = File.AppendText(outputFilePath))
            {
                foreach (string line in OutputData)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        public bool FormattedDataOutput(string TemplateFilePath)
        {
            bool returnValue = true;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                //set thread locals
                SetGlobals();
                //TODO: Validate template
                //Initialize the parser
                TemplateParser templateParser = new TemplateParser(TemplateFilePath);
                templateParser.Parse();
                ValidateExpressions(TemplateFilePath);               

                //Node Parsers
                DataElement dataElement = new DataElement();
                Conditions conditionEvaluator = new Conditions();
                Loops loopEvaluator = new Loops();

                // String list to accumulate output
                List<string> outputStringList = new List<string>();
                //WriteLog("###outputfilename "+ templateParser.GetOutputFileName());
                if(templateParser.AppendToFile() && !singleOutputFileNameMap.ContainsKey(templateParser.GetOutputFileName())){
                   singleOutputFileNameMap.Add(templateParser.GetOutputFileName(), templateParser.GetOutputFileName() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff"));
                }
                string locale = templateParser.GetLocale();
                Globals.Instance.SetData(Constants.LOCALE, locale);

                // Loop through the template and accumulate the output
                while (templateParser.HasNextNode())
                {
                    XmlNode currentNode = templateParser.GetNextNode();
                    switch (templateParser.GetNodeType(currentNode))
                    {
                        case NodeType.Data:
                            outputStringList.AddRange(dataElement.EvaluateData(currentNode));
                            break;
                        case NodeType.If:
                            outputStringList.AddRange(conditionEvaluator.EvaluateCondition(currentNode));
                            break;
                        case NodeType.ForEach:
                            outputStringList.AddRange(loopEvaluator.EvaluateLoop(currentNode));
                            break;
                        default:
                            if (currentNode.NodeType == XmlNodeType.Element)
                            {
                                WriteLog(LOG_PREFIX + "Node type [" + ((XmlElement)currentNode).Name + "] not supported. Will be ignored");
                            }
                            break;
                    }
                }

                writeToFile(templateParser, outputStringList);

                WriteLog(LOG_PREFIX+ " Smart export completed in " + sw.ElapsedMilliseconds+" ms.");

                sw.Stop();

            }
            catch (System.Exception exp)
            {
                returnValue = false;
                WriteLog(LOG_PREFIX + "Error while processing the template file: " + exp.Message);
                WriteLog(exp.StackTrace);

            }
           
            // TODO: Catch the important exceptions here...
            return returnValue;
        }

        ///       <summary>
        ///       The method creates a list of valid DCO references.
        ///      
        private void createDCOPatternList()
        {
            string projectFile = this.BatchPilot.ProjectPath;

            string parentDirectory = Path.GetDirectoryName(projectFile);
            string dcoDefinitionFile = parentDirectory + Path.DirectorySeparatorChar + Path.GetFileName(Path.GetDirectoryName(parentDirectory)) + ".xml";

            XmlDocument batchXML = new XmlDocument();
            batchXML.Load(dcoDefinitionFile);
            XmlElement batchRoot = batchXML.DocumentElement;
            XmlNodeList dcoDocumentNodes = batchRoot.SelectNodes("./D"); //Document nodes
            foreach (XmlNode dcoDocumentNode in dcoDocumentNodes)
            {
                DocumentID = ((XmlElement)dcoDocumentNode).GetAttribute("type");
                XmlNodeList pageList = dcoDocumentNode.SelectNodes("./P");
               
                foreach (XmlNode pageNode in pageList)
                {
                    string pageID = ((XmlElement)pageNode).GetAttribute("type");
                    XmlNodeList allPageList = batchRoot.SelectNodes("./P");
                    XmlNode currentPage = pageNode;
                    foreach (XmlNode page in allPageList)
                    {
                        if (pageID == ((XmlElement)page).GetAttribute("type"))
                        {
                            currentPage = page;
                            break;
                        }
                    }
                    XmlNodeList fieldList = currentPage.SelectNodes("./F");
                    foreach (XmlNode fieldNode in fieldList)
                    {
                        string fieldID = ((XmlElement)fieldNode).GetAttribute("type");
                        string dcoPattern = DocumentID + "." + pageID + "." + fieldID;

                        DCOPatterns.Add(dcoPattern);

                    }
                }
            }
        }

        ///       <summary>
        ///       The method checks if valid DCO references are used in the template file. If an invalid reference if found an exception is thrown.
        ///       <param name="TemplateFile">Fully qualified path of the template file.</param>
        ///       </summary>
        private void ValidateExpressions(string TemplateFile)
        {

            createDCOPatternList();
            byte[] bytes = System.IO.File.ReadAllBytes(TemplateFile);
            string text = System.Text.Encoding.UTF8.GetString(bytes);

            Regex rg = new Regex(Constants.DCO_REF_PATTERN);
            MatchCollection matchedPatterns = rg.Matches(text);

            foreach (Match Pattern in matchedPatterns)
            {
                string DCOTree = Pattern.Value;
                DCOTree = DCOTree.Replace("[", "").Replace("]", "").Replace("DCO.", "");
                if (!DCOPatterns.Contains(DCOTree))
                {
                    WriteLog(LOG_PREFIX + "DCO reference is invalid. " +
                                Pattern.Value);
                    throw new System.ArgumentException("DCO reference invalid. Check detailed logs", Pattern.Value);
                }
            }
        }

    }
}