using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SmartExportTemplates.Utils;

namespace SmartExportTemplates.TemplateCore
{
    class Loops
    {
        protected TDCOLib.IDCO CurrentDCO = (TDCOLib.IDCO)Globals.Instance.GetData(Constants.GE_CURRENT_DCO);
        SmartExportTemplates.SmartExport ExportCore = (SmartExportTemplates.SmartExport)Globals.Instance.GetData(Constants.GE_EXPORT_CORE);

        public Loops()
        {

        }

        ///       <summary>
        ///       The method Evaluates for loop.
        ///       <param name="loopNode">XML node of Foreach</param>
        ///       </summary>
        public List<string> EvaluateLoop(XmlNode loopNode)
        {
           
            List<string> output = new List<string>();

            DataElement dataElement = new DataElement();
            Conditions conditionEvaluator = new Conditions();

            int forEachlevel = getIntValueForEachObjectType(loopNode.Attributes["select"].Value);
            validateForLoop(forEachlevel);

           // ExportCore.WriteLog(" ##### " +CurrentDCO.NumOfChildren());
            for (int i = 0; i < CurrentDCO.NumOfChildren(); i++){

              //  ExportCore.WriteLog(" ##### " +CurrentDCO.GetChild(i).ID);
               //setting the currentIterationDCO , so that it can be used in DCODataRetreiver to get the data.
               Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, CurrentDCO.GetChild(i));

                foreach (XmlNode node in loopNode.ChildNodes)
                {
                    switch(node.Name){
                        case Constants.NodeTypeString.SE_IF:
                            output.AddRange(conditionEvaluator.EvaluateCondition(node));
                            break;
                        case Constants.NodeTypeString.SE_DATA:
                            output.AddRange(dataElement.EvaluateData(node));
                            break;
                        default:
                            if (node.NodeType == XmlNodeType.Element)
                            {
                                ExportCore.WriteLog( "Node type [" + ((XmlElement)node).Name + "] not supported. Will be ignored");
                            }
                            break;
                    }
                }
               //setting it to empty after every iteration.
               Globals.Instance.SetData(Constants.forLoopString.CURRENTITERATIONDCO, Constants.EMPTYSTRING);
            }
            
            return output;
        }

         ///       <summary>
        ///       The method returns integer value of the object type passed.
        ///       <param name="objectType">String value of the level</param>
        ///       </summary>
        private int getIntValueForEachObjectType(String objectType){
            int output = 4;
            switch(objectType.ToUpper()){
            case Constants.forLoopString.BATCH:
                    output = Constants.Batch;
                    break;
            case Constants.forLoopString.DOCUMENT:
                    output = Constants.Document;
                    break;
            case Constants.forLoopString.PAGE:
                    output = Constants.Page;
                    break;
            case Constants.forLoopString.FIELD:
                    output = Constants.Field;
                    break;
            }
            return output;
        }

          ///       <summary>
        ///       The method validates for loop expression.
        ///       <param name="forEachlevel">integer value of the level</param>
        ///       </summary>
        private void validateForLoop(int forEachlevel){

            if(forEachlevel == 4 || forEachlevel == Constants.Batch){
                throw new SmartExportException(Constants.LOG_PREFIX + " Assigned level of ForEach loop is wrong"); 
            }
           if(forEachlevel == CurrentDCO.ObjectType()){
                throw new SmartExportException(Constants.LOG_PREFIX + " ForEach loop level must not be same to the Datacap assigned level"); 
            }
           if(!(forEachlevel == (CurrentDCO.ObjectType()+1))){
                throw new SmartExportException(Constants.LOG_PREFIX + " ForEach loop must be one level lower from the Datacap assigned level "); 
            }

        }
    }
}
