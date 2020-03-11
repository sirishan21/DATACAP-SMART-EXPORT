using System;
using System.IO;
using SmartExportTemplates.TemplateCore;
using System.Collections.Generic;
using SmartExportTemplates.Utils;

namespace SmartExportTemplates.Utils
{   
	//This is the util class for smart export
    public static class SmartExportUtil
    {
		public static List<String> addToOutPutList(List<String> currentOutputList, List<String> outputList) {
			outputList.AddRange(currentOutputList);
			if (outputList.Count >= Constants.GE_DEFAULT_MEMORY_SIZE) {
				writeTempFile(outputList);
				outputList.Clear();
			}
			return outputList;
		}

		public static void writeTempFile(List<string> OutputData)
		{
			Dictionary<string, string> tempFileNameMap = (Dictionary<String, String>)Globals.Instance.GetData(Constants.GE_TEMP_FILE_MAP);
			TemplateParser templateParser = (TemplateParser)Globals.Instance.GetData(Constants.GE_TEMPLATE_PARSER);

			if (!tempFileNameMap.ContainsKey(templateParser.GetOutputFileName()))
			{
				string tempFilePath = Path.Combine(templateParser.GetOutputDirectory() ,
					Constants.GE_TEMP_FILE_PREFIX + templateParser.GetOutputFileName() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fffffff"));
				tempFileNameMap.Add(templateParser.GetOutputFileName(), tempFilePath);
			}
			Globals.Instance.SetData(Constants.GE_TEMP_FILE_MAP, tempFileNameMap);
			

			using (StreamWriter outputFile = File.AppendText(tempFileNameMap[templateParser.GetOutputFileName()]))
			{
				foreach (string line in OutputData)
				{
					outputFile.WriteLine(line);
				}
			}
		}
	}

}

