# Clarion Dictionary Versoniser

A little tool to help with version control of a clarion DCT

# Requirements:

DotNet 4.5, Newtonsoft Json.Net (http://www.newtonsoft.com/json), CommandLine (http://commandline.codeplex.com/)

# Command line:

To get help for command line options

DCTVersioniser.exe --help

# Instructions

A write up of the tool can be found here: https://clarionhub.com/t/how-to-compare-dct-version-using-dctversioniser/1090

This program can be used either by passing parameters from the command line or by just running the application without any parameters.

On first run the appliction will want you to locate your clarion 10 bin.

You will then get an open file dialog which allows you to select DCT or JSON files.

If you select a DCT the application will create a json file of the DCT for you.
If you select a JSON dictionary file, the application will create a dct with the same name as the JSON file, using the JSON file to create the dct.

# Updates
2017-03-27 10:40 - Versioniser will now create two JSON files when versioning a dct file. The original file as already specified.
                   A history file in DCT-History folder.
2017-03-27 11;21 - Added ability to turn on/off history folder saving. It will be off as default but can be turned on/off by using the commandline DctVersioniser -h