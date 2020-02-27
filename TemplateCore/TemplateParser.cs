using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;
using System.Globalization;

namespace SmartExportTemplates.TemplateCore
{
    class TemplateParser
    {
        private string  TemplateFilePath = null;
        XmlElement      TemplateRoot = null;
        XmlNode         CurrentNode = null;
        bool            HasMoreNodes = false;
        XmlNamespaceManager NameSpcManager = null;
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        
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
                this.NameSpcManager = new XmlNamespaceManager(templateXML.NameTable);
                this.NameSpcManager.AddNamespace(Constants.SE_NAMESPACE_NAME, Constants.SE_NAMESPACE_URL);
                this.TemplateRoot = templateXML.DocumentElement;
                if (this.TemplateRoot.HasChildNodes)
                {
                    this.HasMoreNodes = true;
                    this.CurrentNode = TemplateRoot.FirstChild;
                }
            } catch (Exception exp)
            {
                ExportCore.WriteLog(Globals.Instance.GetData("LogPrefix") + "Error while parsing the template file, terminating. Details: " + exp.Message);
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
                case Constants.NodeTypeString.SE_DATA:
                    nodeType = SmartExport.NodeType.Data;
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

        public string GetOutputFileName()
        {
            string OutputFileName = Constants.GE_DEF_OUTPUT_FILE;
            XmlNode outputFileNode = TemplateRoot.GetElementsByTagName(Constants.SE_OUTPUT_FILE_NAME)[0];
            if (outputFileNode != null)
            {
                OutputFileName = (outputFileNode.InnerText != null && !outputFileNode.InnerText.Trim().Equals("")) ?
                                    outputFileNode.InnerText.Trim() : Constants.GE_DEF_OUTPUT_FILE;
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
            string locale = Constants.LOCALE;
            XmlNode localeNode = TemplateRoot.GetElementsByTagName(Constants.SE_LOCALE)[0];
            if (localeNode != null)
            {
                locale = (localeNode.InnerText != null && !localeNode.InnerText.Trim().Equals("")) ?
                                    localeNode.InnerText.Trim() : CultureInfo.CurrentUICulture.Name;
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
                string path = outputFolderNode.InnerText.Trim();
                if (path != null && !path.Equals(""))
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
                            ExportCore.WriteLog(Constants.GE_LOG_PREFIX + "Invalid output folder path provided, using batches folder as output dir.");
                        }
                    }
                }
            }
            return OutputFolder;
        }

    }
}
