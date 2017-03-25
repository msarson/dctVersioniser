# dctVersioniser
Clarion Dictionary Versoniser
DCTVersioniser

A little tool to help with version control of a clarion DCT

Requirements:

DotNet 4.5

Commandline:

DCTVersioniser.exe

On first run the appliction will want you to locate your clarion 10 bin.

You will then get an open file dialog which allows you to select DCT or JSON files.

If you select a DCT the application will create a json file of the DCT for you.
If you select a JSON dictionary file, the application will create a dct with the same name as the JSON file, using the JSON file to create the dct.
