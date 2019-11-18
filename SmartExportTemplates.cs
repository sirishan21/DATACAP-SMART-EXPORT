//
// Licensed Materials - Property of IBM
//
// 5725-C15
// © Copyright IBM Corp. 1994, 2019 All Rights Reserved
// US Government Users Restricted Rights - Use, duplication or
// disclosure restricted by GSA ADP Schedule Contract with IBM Corp.
//
// This is an example of a .NET action for IBM Datacap using .NET 4.0.
// The compliled DLL needs to be placed into the RRS directory.
// The DLL does not need to be registered.  
// Datacap studio will find the RRX file that is embedded in the DLL, you do not need to place the RRX in the RRS directory.
// If you add references to other DLLs, such as 3rd party, you may need to place those DLLs in C:\RRS so they are found at runtime.
// If Datacap references are not found at compile time, add a reference path of C:\Datacap\DCShared\NET to the project to locate the DLLs while building.
// This template has been tested with IBM Datacap 9.0.  

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

namespace SmartExportTemplates
{
    public class SmartExport // This class must be a base class for .NET 4.0 Actions.
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

        // Global constants
        readonly string LOG_PREFIX = "DBA-SmartExport - ";
        readonly string DCO_REF_PATTERN = "\\[DCO.*\\]";

        /// <summary/>
        /// Processes the template file "TemplateFilePath" against the DCO of the current document. 
        /// Execution of this method to be called at the Export step of the workflow when the data is already extracted
        /// Output is written to a file with "OutputFilePrefix" followed by document id/name and date/time for uniqueness
        public bool ConditionalSmartExport(string TemplateFilePath, string OutputFilePrefix)
        {
            bool bResponse = true;

            // Extract the root element of the template file 
            XmlElement templateRoot = null;
            try
            {
                XmlDocument templateXML = new XmlDocument();
                templateXML.Load(TemplateFilePath);
                templateRoot = templateXML.DocumentElement;
            }
            catch (System.IO.FileNotFoundException exp)
            {
                WriteLog(LOG_PREFIX + "Unable to read the template file [" + TemplateFilePath + "]. Error: " + exp.ToString());
                bResponse = false;
            }

            if (!bResponse)
                return bResponse;  // Don't proceed further if template file is not readable

            string batchXMLFile = this.BatchPilot.DCOFile;
            try
            {
                /// Parse the DCO XML file for the current batch and for each document in the batch, 
                /// process the document's exported XML data against the input template to generate 
                /// data output file
                XmlDocument batchXML = new XmlDocument();
                batchXML.Load(batchXMLFile);
                XmlElement batchRoot = batchXML.DocumentElement;
                XmlNodeList dcoDocumentNodes = batchRoot.SelectNodes("./D"); //Document nodes
                foreach(XmlNode dcoDocumentNode in dcoDocumentNodes)
                {
                    XmlNode dataFileNode = dcoDocumentNode.SelectSingleNode("./P/V[@n='DATAFILE']"); //Multiple nodes with V, get the data file node
                    XmlNodeList pageList = dcoDocumentNode.SelectNodes("./P");
                    if (pageList.Count == 0)
                        continue;

                    Dictionary<string, string> pageNameDict = new Dictionary<string, string>();
                    foreach(XmlNode pageNode in pageList)
                    {
                        string pageType = pageNode.SelectSingleNode("./V[@n='TYPE']").InnerText;
                        string pageID = ((XmlElement)pageNode).GetAttribute("id");
                        pageNameDict[pageType] = pageID;
                    }

                    bResponse = ProcessDocument(pageNameDict,
                                                templateRoot,
                                                OutputFilePrefix);   
                }
            } catch (System.IO.FileNotFoundException exp)
            {
                WriteLog(LOG_PREFIX + "DCO XML file not readable. Error: " + exp.ToString());
                bResponse = false;
            } catch (System.Exception exp)
            {
                WriteLog(LOG_PREFIX + "Internal error occurred while processing the application DCO file. Error: " + exp.ToString());
                bResponse = false;
            }
            
            return bResponse;
        }

        /// <summary>
        /// Read the data XML file and extract the document root. Transform Data needs both the template's root element
        /// and the data XMl root element
        /// </summary>
        /// <param name="DataFileName">Data file name for the given document</param>
        /// <param name="BasePath">Base path where the data file resides</param>
        /// <param name="TemplateFilePath">Path to the template file. Full path to the file</param>
        /// <param name="OutputFilePrefix">Prefix for the generated file </param>
        private bool ProcessDocument(Dictionary<string, string> pageNameDict,
                                        XmlElement TemplateRoot,
                                        string OutputFilePrefix)
        {
            bool bResponse = true;
            // Transform the data
            List<string> outputData = TransformData(pageNameDict, TemplateRoot);
            // Write to output file
            string outputFileName = OutputFilePrefix + DataFileName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff");
            string outputFilePath = Path.Combine(BasePath, outputFileName);
            using (StreamWriter outputFile = new StreamWriter(outputFilePath))
            {
                foreach (string line in outputData)
                {
                    outputFile.WriteLine(line);
                }
            }

            return bResponse;
        }

        /// <summary>
        /// Transform the data output to the specified template. 
        /// </summary>
        /// <param name="DataRoot"></param>
        /// <param name="TemplateRoot"></param>
        private List<string> TransformData(Dictionary<string, string> pageNameDict, 
                                            XmlElement TemplateRoot)
        {
            List<string> outputData = new List<string>();
            string tsOutput = null;

            //Write Header
            string header = TemplateRoot.SelectSingleNode("./header").InnerText;
            tsOutput = Regex.Replace(header, DCO_REF_PATTERN, m => getDCOValue(pageNameDict, m.Value));
            outputData.Add(tsOutput);

            //Write Statements
            XmlNodeList statementNodes = TemplateRoot.SelectNodes("./body/statement");
            foreach (XmlNode statementNode in statementNodes)
            {
                outputData.Add(getStatementOutput(statementNode));
            }

            //Write Footer
            string footer = TemplateRoot.SelectSingleNode("./footer").InnerText;
            tsOutput = Regex.Replace(footer, DCO_REF_PATTERN, m => getDCOValue(pageNameDict, m.Value));
            outputData.Add(tsOutput);

            return outputData;
        }

        /// <summary>
        /// Given a DCO reference in the predefined format ([DCO.Page.Field]), this method returns the value
        /// of the DCO. It looks up the exported data to find the value
        /// </summary>
        /// <param name="DCOTree"></param>
        /// <returns></returns>
        private string getDCOValue(Dictionary<string, string> pageNameDict, 
                                        string DCOTree)
        {
            dcSmart.SmartNav localSmartObj = null;
            localSmartObj = new dcSmart.SmartNav(this);

            string output = DCO.FindChild("TM000001").FindChild("Total_Cost").Text;

            return DCOTree.Replace("[", "_").Replace("]", "_" + output);
        }

        private string getStatementOutput(XmlNode StatementNode)
        {
            return ((XmlElement)StatementNode).GetAttribute("name");
        }

        
    }
}
