using System;
using System.IO;
using SmartExportTemplates.TemplateCore;
using System.Collections.Generic;
using SmartExportTemplates.Utils;

namespace SmartExportTemplates.Utils
{   
	//This is the util class for smart export
    public class SmartExportUtil
    {
        private List<String> outputStringList = null;

        private TemplateParser templateParser = null;       
        //It holds temp file paths with key as output file name.
        Dictionary<string, string> tempFileNameMap = new Dictionary<String, String>();

        private SmartExportTemplates.SmartExport exportCore = null;

        //this method sets context for this util class, this is called for every iteration
        //and for every iteration output list is reseted
        public void setContext(TemplateParser parser) {
            outputStringList = new List<String>();
            this.templateParser = parser;
            this.exportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);
        }

        //this method is used to add data to list and flushes data to temp file if size is more than default size.
        public void addToOutPutList(String outputData) {
	
			outputStringList.Add(outputData);
			if (outputStringList.Count >= templateParser.GetOutputMemorySize()) {
				writeTempFile();
				outputStringList.Clear();
			}
		}
        //this methods write data to temp file.
		public void writeTempFile()
		{

			if (!tempFileNameMap.ContainsKey(templateParser.GetOutputFileName()))
			{
				string tempFilePath = Path.Combine(templateParser.GetOutputDirectory() ,
					Constants.GE_TEMP_FILE_PREFIX + templateParser.GetOutputFileName() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff"));
				tempFileNameMap.Add(templateParser.GetOutputFileName(), tempFilePath);
			}

            createOrAppendToFile(tempFileNameMap[templateParser.GetOutputFileName()]);
           
		}

        public void writeToFile(Dictionary<String,String> singleOutputFileNameMap)
        {
            try
            {
                // this ckeck is done to prevent empty file getting generated
                if (outputStringList.Count == 0 && !tempFileNameMap.ContainsKey(templateParser.GetOutputFileName()))
                {
                    exportCore.WriteLog("Empty content. Skipping writing to file: " + templateParser.GetOutputFileName());
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
                //If append to file is false and one iteration is complete, then code will rename temp file to actual file name
                //and delete the entry in the map, so in next iteration new temp file is created for this template 
                //as append to file is false
                if (!templateParser.AppendToFile() && tempFileNameMap.ContainsKey(templateParser.GetOutputFileName()))
                {
                    string tempPath = tempFileNameMap[templateParser.GetOutputFileName()];
                    createOrAppendToFile(tempPath);
                    File.Move(tempPath, outputFilePath);
                    tempFileNameMap.Remove(templateParser.GetOutputFileName());
                    return;
                }

                //if appendtofile is true and temp map has an entry for temp file, for first iteration data is written in temp file 
                //and then renamed to actual file, for all next iteration the data from the list is added to the actual file 
                //not in temp file.
                //this condition will pass only for first iteration.
                if (templateParser.AppendToFile() && tempFileNameMap.ContainsKey(templateParser.GetOutputFileName()) && !File.Exists(outputFilePath))
                {
                    string tempPath = tempFileNameMap[templateParser.GetOutputFileName()];
                    createOrAppendToFile(tempPath);
                    File.Move(tempPath, outputFilePath);
                    tempFileNameMap[templateParser.GetOutputFileName()] = outputFilePath;
                    return;
                }

                //if there are no temp file created then data is flushed to output file here.
                createOrAppendToFile(outputFilePath);
            }
            catch (System.Exception exp)
            {
                exportCore.WriteErrorLog(exp.StackTrace);
                throw new SmartExportException("Error while writing output to file: " + exp.Message);
            }

        }

        //this method is used to create or append data to given file
        //if AppendToFile is false then everytime new file is given then it creates a new file.
        //if AppendToFile is true then everytime singleOutputFileName file is given then it appends to the same file.
        private void createOrAppendToFile(String outputFilePath) {
            try
            {
                using (StreamWriter outputFile = File.AppendText(outputFilePath))
                {
                    foreach (string line in outputStringList)
                    {
                        outputFile.WriteLine(line);
                    }
                }
            }
            catch (System.Exception exp)
            {
                exportCore.WriteErrorLog(exp.StackTrace);
                throw new SmartExportException("Error while writing output to file: " + outputFilePath);
            }
        }

        public string escapeString(string output)
        {

            return escapeString(output,"");
        }

        public string escapeString(string output, string separator)
        {
            string escapedString = output;
            if (templateParser.GetOutputFileExt().Equals("csv", StringComparison.InvariantCultureIgnoreCase) 
                || separator==Constants.COMMA)
            {
                escapedString = "\"" + output + "\"";
            }
            return escapedString;
        }
    }

}

