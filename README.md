# Smart template export

## Description

* Takes in a template file as input and generated formatted output one for each document processed
* Refer to SmartExport.xml for a sample template
* At this moment, only conditions and DCO references are allowed in the template file
* Template syntax:

  * Contains a "header" and a "footer" which allows the user to place in some static (or use DCO variables in there) to structure the data output file
  * The body is formed by a sequence of "statements". Each statement has a set of mutually exclusive rules and a "default". This allows the user to assign the final value to be used in the statement. "output" node contains static + variable that is assigned post the condition evaluation. Data is the "output" node is 
  * Conditions will be written in pre-defined syntax and can use DCO structure to fill in. 
  * Conditions and assignments (values) are evaluated against the extracted data and the output file is created exactly like the user wants it to be
  * The custom action will take the template file as input (and an optional output file prefix) and will generate one output data file for each document that is processed within a batch.

* Output file is written to the current batch directory

## Generate binary and deploy

* Checkout this repo and open the solution in Visual studio. Visual studio should be on the same system where DataCap is installed
* Build the solution. It generates a DLL file in the bin dir of the solution.
* Copy the DLL into the RSS directory of DataCap
* Open DataStudio and it should appear in the global actions (Right click and click information and it should have some guideline as well)

## Usage

* At this moment, this custom action is supposed to be executed in the Export step of the DataCap workflow
* Add the custom action to an existing or new Ruleset. Fill in the below two parameters 
  * Template file should have the input template file with complete path
  * Output prefix is optional and can have a prefix that will be added to the generated output file name
* Run DataCap workflow as usual and post completion, find output files (one for each document) in the current batch folder

## Limitations

**Disclaimer**: This is **NOT** production ready software. It is a POV to better understand broader requirements and its usage

* Conditional logic in the template is evaluated using a library in C#. This is not extensible and reduces the flexibility in writing conditions. At this moment, conditions entered in the template are processed to replace DCO reference and then evaluated as native C# condition (with the exception of replacing "and" and "or" with the constructs). 
* Given the way the conditions are executed, it is expected that the DataCap developer uses it with caution. Once a full version of this custom action is developed, a readme with all that is supported in conditions will be provided as a guide
* Looping constructs not available. A lexical parser implementation is costly and will be implemented (or decided to implement) once requirements are finalized and signed off.
* Given that this is written in a couple of weeks, it is highly recommended not to deploy this in any production environments.
