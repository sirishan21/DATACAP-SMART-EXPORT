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

        Dictionary<string, string> tempFileNameMap = new Dictionary<String, String>();

        private SmartExportTemplates.SmartExport exportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);

        public void setContext(TemplateParser parser) {
            outputStringList = new List<String>();
            this.templateParser = parser;
        }

        public void addToOutPutList(String outputData) {
	
			//List<string> outputStringList = (List<String>)Globals.Instance.GetData("outputStringList");
			outputStringList.Add(outputData);
			if (outputStringList.Count >= templateParser.GetOutputMemorySize()) {
				writeTempFile();
				outputStringList.Clear();
			}
			//Globals.Instance.SetData("outputStringList", outputStringList);
		}

		public void writeTempFile()
		{
			//Dictionary<string, string> tempFileNameMap = (Dictionary<String, String>)Globals.Instance.GetData(Constants.GE_TEMP_FILE_MAP);
			//TemplateParser templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);

			if (!tempFileNameMap.ContainsKey(templateParser.GetOutputFileName()))
			{
				string tempFilePath = Path.Combine(templateParser.GetOutputDirectory() ,
					Constants.GE_TEMP_FILE_PREFIX + templateParser.GetOutputFileName() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff"));
				tempFileNameMap.Add(templateParser.GetOutputFileName(), tempFilePath);
			}
            //Globals.Instance.SetData(Constants.GE_TEMP_FILE_MAP, tempFileNameMap);

            createOrAppendToFile(tempFileNameMap[templateParser.GetOutputFileName()]);
           
		}

        public void writeToFile(Dictionary<String,String> singleOutputFileNameMap)
        {
           // Dictionary<string, string> tempFileNameMap = (Dictionary<String, String>)Globals.Instance.GetData(Constants.GE_TEMP_FILE_MAP);
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
                File.Move(tempPath, outputFilePath);
                tempFileNameMap.Remove(templateParser.GetOutputFileName());
            }

            //if appendtofile is true and temp map has an entry of the that, for first iteration it is written in temp file 
            //and then renamed to actual file, for all next iteration the data from the list is added to the actual file 
            //not in temp file.
            if (templateParser.AppendToFile() && tempFileNameMap.ContainsKey(templateParser.GetOutputFileName()))
            {
                string tempPath = tempFileNameMap[templateParser.GetOutputFileName()];
                if (!File.Exists(outputFilePath))
                {
                    File.Move(tempPath, outputFilePath);
                    tempFileNameMap[templateParser.GetOutputFileName()] = outputFilePath;
                }

            }

            createOrAppendToFile(outputFilePath);
        }

        //this method is used to create or append data to given file
        //if AppendToFile is false then everytime new file is given then it creates a new file.
        //if AppendToFile is true then everytime singleOutputFileName file is given then it appends to the same file.
        private void createOrAppendToFile(String outputFilePath) {
            using (StreamWriter outputFile = File.AppendText(outputFilePath))
            {
                foreach (string line in outputStringList)
                {
                    outputFile.WriteLine(line);
                }
            }
        }
    }

}

