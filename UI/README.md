# DBA-DataCap-SmartExport-UI
GUI for the DataCap Smart Export tool

#Pre-Requesties

1)Node.js
2)Ant
3)Place the following jars in the eclipse dropins folder
com.ibm.ecm.icn.plugin_2.0.3.jar
com.ibm.ecm.icn.plugin.source_2.0.3.jar


#Building the  plugin

1)Change 'npm_path' in build.properties and build.xml file to the location where Node.js is installed in local machine
generally npm_path will be -
Mac : /usr/local/bin
Windows : C:/Program Files/nodejs
2)Run build.xml using ANT. Make sure your computer can connect to internet as the build script will download some dependencies from NPM registry.
3)The build script will generate a plugin jar file.
 
For additional details regarding eclipse plugin development and the dependency jars refer - http://www.redbooks.ibm.com/Redbooks.nsf/RedpieceAbstracts/sg248055.html?Open
- https://www.ibm.com/support/pages/getting-started-new-eclipse-plug-ibm-content-navigator-development
   

#Install plugin and view the feature

1)Open ICN admin desktop
2)Plug-ins->New Plug-in
3)Fill in the plugin jar file path and click "Load".Save
4)Open ICN desktop configuration from "Desktops"
5)Open "Layout" tab. Select the new feature from the feature list. Save the desktop.
6)Refresh Browser of the ICN Desktop configured with the plugin. You will see the new sample feature. 




