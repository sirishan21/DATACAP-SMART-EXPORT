# Smart template export

## Description

1.	It is a custom action that takes an XML based template file as input and generates formatted output file as dictated by the template
2.	Refer to the documentation folder for sample templates and for template documentation - Release/DataCapSmartExport_v1.0.docx


## Build and deploy (for developers)

1.	Checkout this repo and open the solution in Visual studio. Visual studio should be on the same system where DataCap is installed
2.	Build the solution. It generates a DLL file in the bin directory of the solution.
3.	Copy the DLL into the RSS directory of DataCap
4.	Open DataCap studio and find the action library under global actions


## Deploy (for end-users)

1.	Copy the supplied DLL into the RSS directory of DataCap
2.	Open DataCap studio and find the action library under global actions


## Usage

1.	Add the custom action to an existing or new Ruleset and set the parameter of the custom action to the template file path location.
2.	Run DataCap workflow as usual and post completion, find output files in the output directory specified in the template. By default it writes to the batch directory.


## Features

1.	Configuration :
Enables one to  specify the configuration details like the output file name, extension, locale, output directory and memCacheLines – maximum number of lines of data that can be stored in memory .

2.	Data elements :
Enables one to specify the content to be printed in the output file, this could be plain text of field values extracted from the input file.

3.	Looping constructs :
Enables one to execute a set of instructions for a specific number of times. This feature is supported at the batch, document, and page level of the DCO hierarchy. Data elements, looping constructs and conditional constructs can be nested within this.

4.	Conditional constructs :
Enables one to execute a set of instructions if some criteria is satisfied. This feature is supported at the document, page and field level of the DCO hierarchy. Data elements, looping constructs and conditional constructs can be nested within this.

5.	Smart Parameters :
Enables one to use the values of the DataCap defined variables within a file name or print their value in the file.

6.	Tables :
Enables one to extract table content. This feature can be used at file, document, page and field level.

6.	Smart Export Template Builder UI :
Enables one to build a template that is used by the Smart Export Custom action. For further details refer UI/README.md

## Technology
C#
Java
ReactJS

## Dependencies
Datacap.Globals.dll
