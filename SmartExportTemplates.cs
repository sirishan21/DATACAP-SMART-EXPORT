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

        string logPrefix = "DBA-SmartExport - ";
        /// <summary/>
        /// Processes the template file "TemplateFilePath" against the DCO of the current document. 
        /// Execution of this method to be called at the Export step of the workflow when the data is already extracted
        /// Output is written to a file with "OutputFilePrefix" followed by document id/name and date/time for uniqueness
        public bool ConditionalSmartExport(string TemplateFilePath, string OutputFilePrefix)
        {
            bool bResponse = true;
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
                    if (dataFileNode != null)
                    {
                        bResponse = ProcessDocument(dataFileNode.InnerText,
                                                    Path.GetDirectoryName(batchXMLFile),
                                                    TemplateFilePath,
                                                    OutputFilePrefix);
                    }
                }
            } catch (System.IO.FileNotFoundException exp)
            {
                WriteLog(logPrefix + "DCO XML file not readable. Error: " + exp.ToString());
                bResponse = false;
            } catch (System.Exception exp)
            {
                WriteLog(logPrefix + "Internal error occurred while processing the application DCO file. Error: " + exp.ToString());
                bResponse = false;
            }
            
            return bResponse;
        }

        /// <summary>
        /// The main method that processes each of the document data against the template to generate data output file.
        /// Output file is written to the same BasePath. The export folder where all data resides.
        /// </summary>
        /// <param name="DataFileName">Data file name for the given document</param>
        /// <param name="BasePath">Base path where the data file resides</param>
        /// <param name="TemplateFilePath">Path to the template file. Full path to the file</param>
        /// <param name="OutputFilePrefix">Prefix for the generated file </param>
        private bool ProcessDocument(string DataFileName, 
                                        string BasePath,
                                        string TemplateFilePath,
                                        string OutputFilePrefix)
        {
            bool bResponse = true;
            // Extract the root element of the data file for the given document
            XmlElement dataRoot = null;
            try
            {
                XmlDocument dataXML = new XmlDocument();
                dataXML.Load(Path.Combine(BasePath, DataFileName));
                dataRoot = dataXML.DocumentElement; 
            } catch (System.IO.FileNotFoundException exp)
            {
                WriteLog(logPrefix + "Unable to read data XML file [" + DataFileName + "]. Error: " + exp.ToString());
                bResponse = false;
            }
            // Read the template file and process it against the data file.
            XmlElement templateRoot = null;
            try
            {
                XmlDocument templateXML = new XmlDocument();
                templateXML.Load(TemplateFilePath);
                templateRoot = templateXML.DocumentElement;
            } catch (System.IO.FileNotFoundException exp)
            {
                WriteLog(logPrefix + "Unable to read the template file [" + TemplateFilePath + "]. Error: " + exp.ToString());
                bResponse = false;
            }
            if (bResponse)
                TransformData(dataRoot, templateRoot);
            return bResponse;
        }

        private void TransformData(XmlElement DataRoot, XmlElement TemplateRoot)
        {
            WriteLog(logPrefix + "Ready to transform data output");
        }

        public bool SampleAction(string TemplateFile, string OutputFilePrefix)
        {
             bool bRes = true;
             /*dcSmart.SmartNav localSmartObj = null;

             try
             {
                 // An example of how to support Smart Parameters in a .NET action.  
                 // To support smart parameters, use the string type for the input parameter.  The type can be coverted after smart parameter resolution, if needed.
                 localSmartObj = new dcSmart.SmartNav(this);

                 // If the string contains a smart parameter, MetaWord will return the resolved smart parameter
                 // if not, it will simply return the orignal string.  Note that smart paramters only work with the string type.
                 // Smart parameters will not decrypt encrypted strings when called from custom actions.
                 string smartP1 = localSmartObj.MetaWord(p1);

                 SampleProperty01 = smartP1;
                 SampleProperty02 = p2;
                 SampleProperty03 = p3;
                 SampleProperty04 = p4;
                 SampleProperty05 = p5;
                 SampleProperty06 = p6;
                 SampleProperty07 = p7;
                 SampleProperty08 = p8;
                 SampleProperty09 = p9;
                 SampleProperty10 = p10;
                 SampleProperty11 = p11;
                 SampleProperty12 = p12;

                 // An example of how to determine the level of the current DCO object.
                 if (CurrentDCO.ObjectType() == Level.Batch)
                 {
                     WriteLog("Batch Level");
                 }
                 else if (CurrentDCO.ObjectType() == Level.Document)
                 {
                     WriteLog("Document Level");
                 }
                 else if (CurrentDCO.ObjectType() == Level.Page)
                 {
                     WriteLog("Page Level");
                     string imageName = CurrentDCO.ImageName; // We are at the page level, here is how to obtain the image name.
                     WriteLog("Image name: " + imageName);
                 }
                 else if (CurrentDCO.ObjectType() == Level.Field)
                 {
                     WriteLog("Field Level");
                 }

                 string myValue = localSmartObj.MetaWord("@ID");  // Using MetaWord to resolve smart parameters
                 WriteLog("The current object ID: " + myValue);
                 myValue = localSmartObj.MetaWord("@TYPE");       // Using MetaWord to resolve smart parameters
                 WriteLog("The current object Type: " + myValue);
                 myValue = localSmartObj.MetaWord("@PILOT(BATCHDIR)");       // Using MetaWord to resolve smart parameters
                 WriteLog("The current batch directory: " + myValue);

                 WriteLog("Another way to get the batch directory: " + BatchPilot.BatchDir);

                 // Here is an example of setting a variable in the DCO.  If the variable does not exist, it will be created.
                 bool result = localSmartObj.DCONavSetValue("@B.MyVariable", smartP1);
                 if (result)
                 {
                     WriteLog("MyVariable was set in the DCO at the batch level.");
                 }
                 else
                 {
                     WriteLog("MyVariable was not set in the DCO");
                 }

                 // Sometimes an action will need to set the status of the current DCO object.  For example, a validation action
                 // that is peforming a test on a field object may want to set the status to 1 if a validation fails, in addition
                 // to returning false.  Typically 0 is pass and 1 is fail.  Refer to the IBM Datacap documentation for 
                 // more information regarding common status values.
                 // This is an example of setting the object status.
                 CurrentDCO.Status = 0;

                 // The hierarchical variable hr_locale can be used to override the default locale.  A heirarchtical variable
                 // is special because once set, all DCO objects below it will inherit its value unless it is specifically
                 // overridden at a lower level object.  That means the variable could be set to english at the document level
                 // and all pages and fields under that object will also be english.  A single field level object could set the
                 // variable to another locale, such as Russian, and just that field would be treated as Russian while the page
                 // and other fields are treated as English.
                 string currentLocale = CurrentDCO.get_Variable("hr_locale");   // An example of reading a DCO variable.
                 if (currentLocale.Trim().Length > 0)
                 {
                     WriteLog("The current locale in effect for this DCO object: " + currentLocale);
                 }
                 else
                 {
                     WriteLog("Using the default locale.");
                 }

                 // This is a breif example of creating a new child page object as might be done in a vscan action.
                 // TDCOLib.IDCO oPage = DCO.AddChild(Level.Page, "P1" a unique ID, -1);
                 // An action library must not update the XML directly, but instead use the TDCO object.
                 // If children are deleted, then the action that deletes an object, must be the last action called in
                 // a ruleset.  When an object is deleted, any subsequent actions must be called in a following ruleset.

                 // If necessary, an action can set the task status.  Typically, this might be done if the action
                 // encountered a situation where it is necessary to set the batch status to abort.  
                 // Setting the status should be done with care.
                 // example
                 // RRState.set_Data("nTaskStatus", 0); // Abort = 0, Finished = 2, Hold = 4, Pending = 8   
             }
             catch (Exception ex)
             {
                 // It is a best practice to have a try catch in every action to prevent any unexpected errors
                 // from being thrown back to RRS.
                 WriteLog("There was an exception: " + ex.Message);
             }

             localSmartObj = null;*/
             return bRes; 
            
        }



        string prop1 = "";
        /// <summary/>
        /// This is an example of an action that sets a C# property.  
        /// An action that sets a property always returns true.
        /// The parameter type must match the type defined in the RRX file for this action.
        public string SampleProperty01
        {
            get { return prop1; }
            set
            {
                prop1 = value;
                OutputToLog(5, prop1);
            }
        }

        bool prop2 = false;
        /// <summary/>
        public bool SampleProperty02
        {
            get { return prop2; }
            set
            {
                prop2 = value;
                OutputToLog(5, prop2.ToString());
            }
        }

        int prop3 = 0;
        /// <summary/>
        public int SampleProperty03
        {
            get { return prop3; }
            set
            {
                prop3 = value;
                OutputToLog(5, prop3.ToString());
            }
        }

        uint prop4 = 0;
        /// <summary/>
        public uint SampleProperty04
        {
            get { return prop4; }
            set
            {
                prop4 = value;
                OutputToLog(5, prop4.ToString());
            }
        }

        long prop5 = 0;
        /// <summary/>
        public long SampleProperty05
        {
            get { return prop5; }
            set
            {
                prop5 = value;
                OutputToLog(5, prop5.ToString());
            }
        }

        ulong prop6 = 0;
        /// <summary/>
        public ulong SampleProperty06
        {
            get { return prop6; }
            set
            {
                prop6 = value;
                OutputToLog(5, prop6.ToString());
            }
        }

        short prop7 = 0;
        /// <summary/>
        public short SampleProperty07
        {
            get { return prop7; }
            set
            {
                prop7 = value;
                OutputToLog(5, prop7.ToString());
            }
        }

        ushort prop8 = 0;
        /// <summary/>
        public ushort SampleProperty08
        {
            get { return prop8; }
            set
            {
                prop8 = value;
                OutputToLog(5, prop8.ToString());
            }
        }

        byte prop9 = 0;
        /// <summary/>
        public byte SampleProperty09
        {
            get { return prop9; }
            set
            {
                prop9 = value;
                OutputToLog(5, prop9.ToString());
            }
        }

        sbyte prop10 = 0;
        /// <summary/>
        public sbyte SampleProperty10
        {
            get { return prop10; }
            set
            {
                prop10 = value;
                OutputToLog(5, prop10.ToString());
            }
        }

        float prop11 = 0;
        /// <summary/>
        public float SampleProperty11
        {
            get { return prop11; }
            set
            {
                prop11 = value;
                OutputToLog(5, prop11.ToString());
            }
        }

        double prop12 = 0;
        /// <summary/>
        public double SampleProperty12
        {
            get { return prop12; }
            set
            {
                prop12 = value;
                OutputToLog(5, prop12.ToString());
            }
        }
    }
}
