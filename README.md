# Smart template export

## Description

* Takes a template file as input and generates formatted output file as dictated by the template
* Refer to the documentation folder for sample templates and for template documentation


## Build and deploy (for developers)

* Checkout this repo and open the solution in Visual studio. Visual studio should be on the same system where DataCap is installed
* Build the solution. It generates a DLL file in the bin dir of the solution.
* Copy the DLL into the RSS directory of DataCap
* Open DataCap studio and find the action library under global actions

## Deploy (for end-users)

* Copy the supplied DLL into the RSS directory of DataCap
* Open DataCap studio and find the action library under global actions

## Usage

* Add the custom action to an existing or new Ruleset. Fill in the below two parameters 
  * Supply the template file path as input (full path with filename and extension)
* Run DataCap workflow as usual and post completion, find output files in the selected output directory. By default it writes to the batch directory.

