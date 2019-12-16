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
        struct NodeType
        {
            internal const int Data = 0;
            internal const int Condition = 1;
            internal const int Loop = 2;
        }

        // Global constants
        readonly string LOG_PREFIX = "DBA-SmartExport - ";
        readonly string DCO_REF_PATTERN = "\\[DCO\\..+?\\..+?\\..+?\\]";
        
        //Global variables
        // document ID reference used by the child methods during processing.
        string DocumentID = null;
        // Page type to Page ID dict used to map Page type reference to Page ID
        Dictionary<string, string> PageTypeDict = null;
        // Batch path where the output files are written to
        string BatchDirPath = null;
        // evaluation engine
        Microsoft.JScript.Vsa.VsaEngine EvalEngine = Microsoft.JScript.Vsa.VsaEngine.CreateEngine();


        private void SetGlobals()
        {
            // Set the global references into thread local for use by the different modules
        }

        private int getNodeType(XmlNode currentNode)
        {
            return NodeType.Data;
        }

        private void writeToFile(string OutputFilePrefix,
                                    string OutputDir,
                                    List<string> OutputData,
                                    bool AppendToFile)
        {
            // Write to output file
            string outputFileName = OutputFilePrefix + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff");
            string outputFilePath = Path.Combine(OutputDir, outputFileName);
            using (StreamWriter outputFile = new StreamWriter(outputFilePath))
            {
                foreach (string line in OutputData)
                {
                    outputFile.WriteLine(line);
                }
            }
        }

        public bool FormattedDataOutput (string TemplateFilePath,
                                            string OutputFilePrefix,
                                            string OutputFolder,
                                            bool AppendToFile)
        {
            bool returnValue = true;
            try
            {
                //set thread locals
                SetGlobals();
                //Initialize the parser
                TemplateParser templateParser = new TemplateParser(TemplateFilePath);
                templateParser.Parse();
                //Node Parsers
                DataElement dataElement = new DataElement();
                Conditions conditionEvaluator = new Conditions();
                Loops loopEvaluator = new Loops();

                // String list to accumulate output
                List<string> outputStringList = new List<string>();
                
                // Loop through the template and accumulate the output
                while (templateParser.HasNextNode())
                {
                    XmlNode currentNode = templateParser.GetNextNode();
                    switch(getNodeType(currentNode))
                    {
                        case NodeType.Data:
                            outputStringList.AddRange(dataElement.EvaluateData(currentNode));
                            break;
                        case NodeType.Condition:
                            outputStringList.AddRange(conditionEvaluator.EvaluateCondition(currentNode));
                            break;
                        case NodeType.Loop:
                            outputStringList.AddRange(loopEvaluator.EvaluateLoop(currentNode));
                            break;
                        default:
                            WriteLog(LOG_PREFIX + "Node type [" + ((XmlElement)currentNode).Name + "] not supported. Will be ignored");
                            break;
                    }
                }
                //Write data to output file
                writeToFile(OutputFilePrefix, OutputFolder, outputStringList, AppendToFile);

            } catch (System.Exception exp)
            {
                returnValue = false;
                WriteLog(LOG_PREFIX + "Error while processing the template file: " + exp.Message);
            }
            // TODO: Catch the important exceptions here...
            return returnValue;
        }



        /// <summary/>
        /// Processes the template file "TemplateFilePath" against each of the processed documents
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
            catch (Exception exp)
            {
                WriteLog(LOG_PREFIX + "Unable to parse the template file [" + TemplateFilePath + "]. Error: " + exp.ToString());
                bResponse = false;
            }

            if (!bResponse)
                return bResponse;  // Don't proceed further if template file is not readable

            string batchXMLFile = this.BatchPilot.DCOFile;
            BatchDirPath = Path.GetDirectoryName(batchXMLFile);
            try
            {
                /// Parse the DCO XML file for the current batch and for each document in the batch, 
                /// process the document's exported data against the input template to generate 
                /// data output file
                XmlDocument batchXML = new XmlDocument();
                batchXML.Load(batchXMLFile);
                XmlElement batchRoot = batchXML.DocumentElement;
                XmlNodeList dcoDocumentNodes = batchRoot.SelectNodes("./D"); //Document nodes
                foreach(XmlNode dcoDocumentNode in dcoDocumentNodes)
                {
                    DocumentID = ((XmlElement)dcoDocumentNode).GetAttribute("id");
                    XmlNodeList pageList = dcoDocumentNode.SelectNodes("./P");
                    // Data to be processed is always within pages
                    if (pageList.Count == 0)
                    {
                        WriteLog(LOG_PREFIX + "No data to proces. Processing skipped for document: " + DocumentID);
                        continue;
                    }
                        
                    if (PageTypeDict != null)
                    {
                        PageTypeDict.Clear();
                    }
                    else
                    {
                        PageTypeDict = new Dictionary<string, string>();
                    }

                    foreach(XmlNode pageNode in pageList)
                    {
                        string pageType = pageNode.SelectSingleNode("./V[@n='TYPE']").InnerText;
                        string pageID = ((XmlElement)pageNode).GetAttribute("id");
                        PageTypeDict[pageType] = pageID;
                    }

                    bResponse = ProcessDocument(templateRoot,
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
        /// TransformData is used for transforming. This method captures and writes the output to the file
        /// </summary>
        /// <param name="TemplateFilePath">Path to the template file. Full path to the file</param>
        /// <param name="OutputFilePrefix">Prefix for the generated file </param>
        private bool ProcessDocument(XmlElement TemplateRoot,
                                        string OutputFilePrefix)
        {
            bool bResponse = true;
            // Transform the data
            List<string> outputData = TransformData(TemplateRoot);
            // Write to output file
            string outputFileName = OutputFilePrefix + DocumentID + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff");
            string outputFilePath = Path.Combine(BatchDirPath, outputFileName);
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
        /// <param name="TemplateRoot"></param>
        private List<string> TransformData(XmlElement TemplateRoot)
        {
            List<string> outputData = new List<string>();
            string tsOutput = null;

            //Write Header
            XmlNode headerNode = TemplateRoot.SelectSingleNode("./header");
            if (headerNode != null)
            {
                string headerText = headerNode.InnerText;
                if (headerText != null)
                {
                    tsOutput = Regex.Replace(headerText, DCO_REF_PATTERN, m => getDCOValue(m.Value));
                    outputData.Add(tsOutput);
                }
            }

            //Write Statements
            XmlNodeList statementNodes = TemplateRoot.SelectNodes("./body/statement");
            if (statementNodes != null)
            {
                foreach (XmlNode statementNode in statementNodes)
                {
                    tsOutput = getStatementOutput(statementNode);
                    if (tsOutput != null)
                        outputData.Add(tsOutput);
                }
            }

            //Write Footer
            XmlNode footerNode = TemplateRoot.SelectSingleNode("./footer");
            if (footerNode != null)
            {
                string footerText = footerNode.InnerText;
                if (footerText != null)
                {
                    tsOutput = Regex.Replace(footerText, DCO_REF_PATTERN, m => getDCOValue(m.Value));
                    outputData.Add(tsOutput);
                }
            }

            return outputData;
        }

        /// <summary>
        /// For each statement node in the template, this method evaluates the rules and finally 
        /// returns the output text replacing condition values and DCO references in it.
        /// </summary>
        /// <param name="StatementNode"></param>
        /// <returns></returns>
        private string getStatementOutput(XmlNode StatementNode)
        {
            string stmtText = null;
            string stmtName = ((XmlElement)StatementNode).GetAttribute("name");
            try
            {
                string stmtValue = null;
                XmlNodeList ruleList = StatementNode.SelectNodes("./rules/rule");
                if (ruleList == null)
                {
                    WriteLog(LOG_PREFIX + "No rules found for statement with name: " + stmtName);
                }
                else
                {
                    // Evaluate the conditions. If more that one matches, the latest condition value is overwritten
                    foreach (XmlNode rule in ruleList)
                    {
                        string ruleName = ((XmlElement)rule).GetAttribute("name");
                        XmlNode conditionNode = rule.SelectSingleNode("./condition");
                        if (conditionNode == null)
                        {
                            WriteLog(LOG_PREFIX + "Condition node not found for rule " + ruleName + " in statement " + stmtName);
                            continue;
                        }
                        if (evaluateCondition(rule.SelectSingleNode("./condition")))
                        {
                            XmlNode valueNode = rule.SelectSingleNode("./value");
                            if (valueNode == null)
                            {
                                WriteLog(LOG_PREFIX + "Value node not present for rule " + ruleName + " in statement " + stmtName);
                                continue;
                            }

                            stmtValue = Regex.Replace(valueNode.InnerText, DCO_REF_PATTERN, m => getDCOValue(m.Value));
                        }
                    }
                }
                // use the default if no conditions satisfies
                if (stmtValue == null)
                {
                    XmlNode defaultNode = StatementNode.SelectSingleNode("./rules/default");
                    if (defaultNode != null) { 
                        string valueText = defaultNode.InnerText;
                        stmtValue = Regex.Replace(valueText, DCO_REF_PATTERN, m => getDCOValue(m.Value));
                    } else
                    {
                        WriteLog(LOG_PREFIX + "No conditions matched for statement " + stmtName + "; No default value provided;" +
                            " value replacement in output will be skipped" );
                    }
                }

                XmlNode outputNode = StatementNode.SelectSingleNode("./output");
                // Read text from output node and super impose value and variables in it.
                if (outputNode != null && outputNode.InnerText != null)
                {
                    stmtText = outputNode.InnerText;
                    
                    // if value was extracted from conditions, replace that first
                    if (stmtValue != null)
                    {
                        stmtText = stmtText.Replace("[value]", stmtValue);
                    }
                    // replace any DCO references in the output text
                    stmtText = Regex.Replace(stmtText, DCO_REF_PATTERN, m => getDCOValue(m.Value));

                } else
                {
                    WriteLog(LOG_PREFIX + "Statement " + stmtName + " does not have output node or it is empty. Statment discarded");
                }

            } catch (Exception exp)
            {
                WriteLog(LOG_PREFIX + "Error while processing statement: " + stmtName);
                WriteLog(LOG_PREFIX + "Detailed error: " + exp.ToString());
            }

            return stmtText;
        }

        /// <summary>
        /// Evaluates the condition by using Jscript Eval library. This is not production ready. TODO
        /// FUTURE: Write a lexical parser to transform the conditions into program constructs.
        /// </summary>
        /// <param name="conditionNode"></param>
        /// <returns>true/false</returns>
        private bool evaluateCondition(XmlNode conditionNode)
        {
            bool response = false;
            string conditionNodeTxt = conditionNode.InnerText;
            if (conditionNodeTxt == null)
            {
                WriteLog(LOG_PREFIX + "Empty condition nodes found. Please check the template file for correctness");
                return response;
            }
            //replace the DCO references before evaluating the condition
            string conditionText = Regex.Replace(conditionNodeTxt, DCO_REF_PATTERN, m => getDCOValue(m.Value));
            conditionText = conditionText.Replace(" and ", " && ");
            conditionText = conditionText.Replace(" or ", " || ");
            try
            {
                response = (bool)Microsoft.JScript.Eval.JScriptEvaluate(conditionText, EvalEngine);
            } catch (Exception exp)
            {
                // Evaluating conditions using this technique is risky (injections)
                // Moreover, this API is deprecated. This is for the timebeing (to prototype and define the POV)
                // If conditions do not evaluate properly, write log and skip the condition
                // TODO: Lexical parser to get the conditions and transform them to c# code constructs
                WriteLog(LOG_PREFIX + "Condition evalution failed for condition:" + conditionText);
                WriteLog(LOG_PREFIX + "Detailed error: " + exp.ToString());
            }
            return response;
        }

        /// <summary>
        /// Given a DCO reference in the predefined format [DCO.<doc_type>.<page_type>.<field_name>], 
        /// this method returns the value of the DCO. It looks up the DCO object to find the value
        /// </summary>
        /// <param name="DCOTree"></param>
        /// <returns>If found returns the value of the reference DCO. Else returns null</returns>
        private string getDCOValue(string DCOTree)
        {
            // DCO reference in the template file should adhere to a 4 part string [DCO.<doc_type>.<page_type>.<field_name>]
            // Parse the DCO reference and extract the page_type and field_name which can then be used to look up in the 
            // current document that is being processed
            DCOTree = DCOTree.Replace("[", "").Replace("]", "");
            char[] sep = { '.' };
            string[] dcoArray = DCOTree.Split(sep, 4, StringSplitOptions.None);

            if (dcoArray.Length != 4)
            {
                WriteLog(LOG_PREFIX + "DCO reference does not confirm to spec. " +
                    "Expected 4 part string like [DCO.<doc_type>.<page_type>.<field_name>]." +
                    "Found: " + DCOTree);
                throw new System.ArgumentException("DCO reference invalid. Check detailed logs", DCOTree);
            }

            // get the page ID for the page type (in the current document being processed)
            string pageID = PageTypeDict[dcoArray[2]];

            // get the value of the DCO reference using the page ID and the field name
            string output = null;
            try
            {
                output = DCO.FindChild(pageID).FindChild(dcoArray[3]).Text;
            } catch (Exception exp)
            {
                // There could be reference in the template for the documents that are not processed in the current batch
                // Template in TravelDocs can have reference to a field under Flight but the current batch doesn't have 
                // any flight related input. Alternatively, Flight and Car Rental gets processed but for the Car Rental 
                // data output, there cannot be any Flight reference
                WriteLog(LOG_PREFIX + "Unable to find DCO reference for the document with ID: " + DocumentID);
                WriteLog(LOG_PREFIX + "Error while reading DCO reference: " + exp.ToString());
            }

            return output;
        }
 
    }
}
