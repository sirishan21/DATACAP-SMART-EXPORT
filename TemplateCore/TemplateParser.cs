using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SmartExportTemplates.TemplateCore
{
    public class TemplateParser
    {
        private string  TemplateFilePath = null;
        XmlElement      TemplateRoot = null;
        XmlNode         CurrentNode = null;
        bool            HasMoreNodes = false;
        XmlNamespaceManager NameSpcManager = null;
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        private dcSmart.SmartNav smartNav = (dcSmart.SmartNav)Globals.Instance.GetData(Constants.GE_SMART_NAV);
        string[] templateContents = null;

        public TemplateParser(string TemplateFilePath)
        {
            this.TemplateFilePath = TemplateFilePath;
            Globals.Instance.SetData(Constants.GE_TEMPLATE_PARSER, this);
        }

        public XmlNamespaceManager getNameSpcManager()
        {
            return this.NameSpcManager;
        }

        public bool Parse() 
        {
            try
            {
                XmlDocument templateXML = new XmlDocument();
                templateXML.Load(TemplateFilePath);
                ExportCore.WriteInfoLog("template file :" + TemplateFilePath);
                this.NameSpcManager = new XmlNamespaceManager(templateXML.NameTable);
                this.NameSpcManager.AddNamespace(Constants.SE_NAMESPACE_NAME, Constants.SE_NAMESPACE_URL);
                this.TemplateRoot = templateXML.DocumentElement;
                if (this.TemplateRoot.HasChildNodes)
                {
                    this.HasMoreNodes = true;
                    this.CurrentNode = TemplateRoot.FirstChild;
                }
                templateContents = System.IO.File.ReadAllLines(TemplateFilePath);

            }
            catch (Exception exp)
            {
                ExportCore.WriteErrorLog("Error while parsing the template file, terminating. Details: " + exp.Message);
                throw new SmartExportException("Error while parsing template file. Please verify the syntax and semantics of the template file.");
            }
            return true;
        }

        public bool HasNextNode()
        {
            return this.HasMoreNodes;
        }

        public XmlNode GetNextNode()
        {
            XmlNode TmpNode = CurrentNode;
            HasMoreNodes = false;
            if (CurrentNode != null && CurrentNode.NextSibling != null)
            {
                CurrentNode = CurrentNode.NextSibling;
                HasMoreNodes = true;
            }

            return TmpNode;
        }

        public int GetNodeType(XmlNode node)
        {
            if (node == null || node.Name == null || node.NodeType != XmlNodeType.Element)
                return SmartExport.NodeType.Invalid;

            int nodeType = SmartExport.NodeType.Invalid;

            switch (node.Name.Trim())
            {
                case Constants.NodeTypeString.SE_IF:
                    nodeType = SmartExport.NodeType.If;
                    break;
                case Constants.NodeTypeString.SE_FOREACH:
                    nodeType = SmartExport.NodeType.ForEach;
                    break;
                case Constants.NodeTypeString.SE_ROWS:
                    nodeType = SmartExport.NodeType.ForEachRows;
                    break;
                case Constants.NodeTypeString.SE_DATA:
                    nodeType = SmartExport.NodeType.Data;
                    break;
                case Constants.NodeTypeString.SE_HEADER:
                    nodeType = SmartExport.NodeType.Header;
                    break;
            }
            return nodeType;
        }

        public bool AppendToFile()
        {
            bool AppendToFile = false;
            XmlNode appendToFileNode = TemplateRoot.GetElementsByTagName(Constants.SE_APPEND_TO_FILE)[0];
            if (appendToFileNode != null)
            {
                string sAppendToFile = appendToFileNode.InnerText.Trim();
                AppendToFile = (sAppendToFile.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
            }
            return AppendToFile;
        }

        public bool CollateBatchOutput()
        {
            bool collateBatchOutput = false;
            XmlNode BatchOutputNode = TemplateRoot.GetElementsByTagName(Constants.SE_BATCH_OUTPUT)[0];
            if (BatchOutputNode != null)
            {
                string collateBatchOutputValue = "false";
                foreach (XmlNode node in BatchOutputNode.ChildNodes)
                {
                    if (Constants.SE_COLLATE == node.Name)
                    {
                        collateBatchOutputValue = node.InnerText.Trim();
                        break;
                    }
                }
                collateBatchOutput =
                    (collateBatchOutputValue.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
            }
            return collateBatchOutput;
        }

        public bool NameBatchOutputAfterInput()
        {
            bool nameAfterInput = false;
            XmlNode BatchOutputNode = TemplateRoot.GetElementsByTagName(Constants.SE_BATCH_OUTPUT)[0];
            if (BatchOutputNode != null)
            {
                string nameAfterInputValue = "false";
                foreach (XmlNode node in BatchOutputNode.ChildNodes)
                {
                    if (Constants.SE_NAME_AFTER_INPUT == node.Name)
                    {
                        nameAfterInputValue = node.InnerText.Trim();
                        break;
                    }
                }
                nameAfterInput =
                    (nameAfterInputValue.Equals("True", StringComparison.InvariantCultureIgnoreCase)) ? true : false;
            }
            return nameAfterInput;
        }

        public string GetOutputFileName()
        {
            string OutputFileName = Constants.GE_DEF_OUTPUT_FILE;
            XmlNode outputFileNode = TemplateRoot.GetElementsByTagName(Constants.SE_OUTPUT_FILE_NAME)[0];
            if (outputFileNode != null)
            {
                string nodeValue = getNodevalue(outputFileNode);
                if(nodeValue != null && !nodeValue.Equals(Constants.EMPTYSTRING))
                {
                   OutputFileName = nodeValue;
                }
            }
            return OutputFileName;
        }

        public string GetOutputFileExt()
        {
            string OutputFileExt = Constants.GE_DEF_OUTPUT_FILE_EXT;
            XmlNode outputFileExtNode = TemplateRoot.GetElementsByTagName(Constants.SE_OUTPUT_FILE_EXTENSION)[0];
            if (outputFileExtNode != null)
            {
                OutputFileExt = (outputFileExtNode.InnerText != null && !outputFileExtNode.InnerText.Trim().Equals("")) ?
                                    outputFileExtNode.InnerText.Trim() : Constants.SE_OUTPUT_FILE_EXTENSION;
            }
            return OutputFileExt;
        }

        public string GetLocale()
        {
            string locale = CultureInfo.CurrentUICulture.Name;
            XmlNode localeNode = TemplateRoot.GetElementsByTagName(Constants.SE_LOCALE)[0];
            if (localeNode != null)
            {
                string nodeValue = getNodevalue(localeNode);
                if(nodeValue != null && !nodeValue.Equals(Constants.EMPTYSTRING))
                {
                   locale = nodeValue;
                }
            }
            return locale;
        }


        public string GetOutputDirectory()
        {
            //TODO - Get the output dir from the template file and validate it for its existence
            // if not found, create it and return the path. Temporarily, returning the batch dir path
            string OutputFolder = (string)Globals.Instance.GetData(Constants.GE_BATCH_DIR_PATH);
            XmlNode outputFolderNode = TemplateRoot.SelectSingleNode("./" + Constants.SE_OUTPUT_DIR_PATH, this.NameSpcManager);
            if (outputFolderNode != null)
            {
                string path = getNodevalue(outputFolderNode);
                if (path != null && !path.Equals(Constants.EMPTYSTRING))
                {
                    if (Directory.Exists(path))
                    {
                        OutputFolder = path;
                    }
                    else
                    {
                        try
                        {
                            OutputFolder = Directory.CreateDirectory(path).FullName;
                        }
                        catch (Exception)
                        {
                            // Log exception and use the batches folder. Ignore the exception
                            ExportCore.WriteErrorLog("Invalid output folder path provided, using batches folder as output dir.");
                        }
                    }
                }
            }
            return OutputFolder;
        }

        //This method is used to get the innertext if its child node text node or 
        // smart parameter value its child node is under smart param node
        private String getNodevalue(XmlNode headerNode){
           StringBuilder nodeValue = new StringBuilder(Constants.EMPTYSTRING);
                foreach (XmlNode node in headerNode.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case Constants.TEXT_NODE_NAME:
                            nodeValue.Append(node.Value.Trim());
                            break;
                        case Constants.SE_SMART_PARAM_NODE_NAME:
                           nodeValue.Append(smartNav.MetaWord(Constants.SMARTP_AT + node.InnerText.Trim()));
                           ExportCore.WriteDebugLog("smart param value for '"+ node.InnerText.Trim() + "' is " + 
                                    smartNav.MetaWord(Constants.SMARTP_AT + node.InnerText.Trim()));
                            break;
                        default:
                            throw new SmartExportException("Internal error. " + node.Name + " node is not supported inside "+ headerNode.Name  +" node ");
                    }
                }
          return nodeValue.ToString();
        }

        
        public int GetOutputMemorySize()
        {
            String OutputMemorySize = Constants.GE_DEFAULT_OUTPUT_MEMORY_CACHE_LINES;
            XmlNode OutputMemorySizeNode = TemplateRoot.GetElementsByTagName(Constants.SE_OUTPUT_MEM_CACHE_LINES)[0];
            if (OutputMemorySizeNode != null)
            {
                OutputMemorySize = (OutputMemorySizeNode.InnerText != null && !OutputMemorySizeNode.InnerText.Trim().Equals("")) ?
                                    OutputMemorySizeNode.InnerText.Trim() :Constants.GE_DEFAULT_OUTPUT_MEMORY_CACHE_LINES;
            }
            return int.Parse(OutputMemorySize);
        }

        public string GetLineNumberForPatterns(List<string> Patterns)
        {
            int counter = 0;
            List<int> lineNumbers = new List<int>();
            foreach (string line in templateContents)
            {
                counter++;
                foreach (string pattern in Patterns)
                {
                    if (line.Contains(pattern))
                    {
                        lineNumbers.Add(counter);
                    }
                }
            }
            return string.Join(",", lineNumbers);
        }

        public string GetLineNumberForNode(XmlNode Node)
        {
            int counter = 0;
            List<int> lineNumbers = new List<int>();

            foreach (string line in templateContents)
            {
                counter++;

                if (Node.Name == Constants.NodeTypeString.SE_FOREACH)
                {
                    Regex rgxFor = new Regex("se:for-each[ ]*select[ ]*=[ ]*\"" + Node.Attributes["select"].Value + "\"[ ]*");
                    if (rgxFor.IsMatch(line))
                        lineNumbers.Add(counter);
                }
                else if (Node.Name == Constants.NodeTypeString.SE_IF)
                {
                    Regex rgxIf = new Regex("se:if[ ]*test[ ]*=[ ]*\"" + ((XmlElement)Node).GetAttribute(Constants.SE_ATTRIBUTE_COND_TEST) + "\"[ ]*");
                    if (rgxIf.IsMatch(line))
                        lineNumbers.Add(counter);
                }
                
            }
            return string.Join(",", lineNumbers);
        }


    }
}