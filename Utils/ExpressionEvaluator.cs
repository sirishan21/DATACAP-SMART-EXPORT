//
// Licensed Materials - Property of IBM
//
// 5725-C15
// © Copyright IBM Corp. 1994, 2019 All Rights Reserved
// US Government Users Restricted Rights - Use, duplication or
// disclosure restricted by GSA ADP Schedule Contract with IBM Corp.
//
// ExpressionEvaluator evaluates expressions based on the underlying data type of the operands.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datacap.Global;
using SmartExportTemplates.TemplateCore;
namespace SmartExportTemplates.Utils
{
    class ExpressionEvaluator
    {
        public static bool evaluateExpression(string operandOne, string operandTwo, string type, string op)
        {
            Dates date = new Dates();
            bool response = false;
            if ((operandOne == "" || operandTwo == "") && type != Constants.DataTypeString.STRING)
            {
                return false;
            }
            switch (type )
            {
                case Constants.DataTypeString.BOOL :
                    response = evaluateBooleanExpression(bool.Parse(operandOne) , bool.Parse(operandTwo),op);
                    break;
                case Constants.DataTypeString.STRING :
                    response = evaluateStringExpression(operandOne,operandTwo,op);
                    break;
                case Constants.DataTypeString.INT  :
                    response = evaluateIntegerExpression(Int64.Parse(operandOne) , Int64.Parse(operandTwo),op);
                    break;
 
                case Constants.DataTypeString.DOUBLE  :
                    response =evaluateDoubleExpression( double.Parse(operandOne), double.Parse(operandTwo),op);
                    break;
                
                case Constants.DataTypeString.DATE_TIME :
                    response = evaluateDateTimeExpression(
                        DateTime.Parse(date.FormatDateTime(operandOne, Constants.DATE_FORMAT, (string) Globals.Instance.GetData(Constants.LOCALE))),
                        DateTime.Parse(date.FormatDateTime(operandTwo, Constants.DATE_FORMAT, (string)Globals.Instance.GetData(Constants.LOCALE))), op);
                    break;
                
            }
            return response;
        }

        public static bool evaluateBooleanExpression(bool operandOne, bool operandTwo,  string op)
        {
             bool response = false;
            
            switch (op)
            {
                case Constants.Operators.EQUALS:
                     response =( operandOne ==  operandTwo);
                    break;
                default:
                    throw new SmartExportException("The operator " + op + " is not applicable for type boolean. " );

            }
            return response;
        }

        public static bool evaluateStringExpression(string operandOne, string operandTwo, string op)
        {
            bool response = false;

            switch (op)
            {
                case Constants.Operators.EQUALS:
                     response = operandOne.Equals(operandTwo, StringComparison.OrdinalIgnoreCase);
                    break;
                case Constants.Operators.CONTAINS:
                    response = operandOne.Split(',').Contains(operandTwo);
                    break;
                default:
                    throw new SmartExportException("The operator " + op + " is not applicable for type string");

            }
            return response;
        }

        public static bool evaluateIntegerExpression(Int64 operandOne, Int64 operandTwo, string op)
        {
            bool response = false;

            switch (op)
            {
                case Constants.Operators.EQUALS:
                    response =  operandOne ==  operandTwo ;
                    break;
                case Constants.Operators.GREATER_THAN:
                    response = operandOne  >  operandTwo ;
                    break;
                case Constants.Operators.LESSER_THAN:
                    response =  operandOne  < operandTwo ;
                    break;
                default:
                    throw new SmartExportException("The operator " + op + " is not applicable for type integer");

            }
            return response;
        }

        public static bool evaluateDoubleExpression(double operandOne, double operandTwo, string op)
        {
            bool response = false;

            switch (op)
            {
                case Constants.Operators.EQUALS:
                    response = operandOne == operandTwo;
                    break;
                case Constants.Operators.GREATER_THAN:
                    response = operandOne > operandTwo;
                    break;
                case Constants.Operators.LESSER_THAN:
                    response = operandOne < operandTwo;
                    break;
                default:
                    throw new SmartExportException("The operator " + op + " is not applicable for type double");

            }
            return response;
        }

        public static bool evaluateDateTimeExpression(DateTime operandOne, DateTime operandTwo, string op)
        {
            bool response = false;

            switch (op)
            {
                case Constants.Operators.EQUALS:
                    response = operandOne == operandTwo;
                    break;
                case Constants.Operators.GREATER_THAN:
                    response = operandOne > operandTwo;
                    break;
                case Constants.Operators.LESSER_THAN:
                    response = operandOne < operandTwo;
                    break;
                default:
                    throw new SmartExportException("The operator " + op + " is not applicable for type DateTime");

            }
            return response;
        }
    }
}
