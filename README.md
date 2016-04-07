# [![BuildAMation](http://buildamation.com/BAM-small.png)](https://github.com/markfinal/BuildAMation) parser package repository

This repository only contains the Bam build scripts for
* flex-2.x
* bison-2.x
* regex-2.7

With these scripts, Flex and Bison are not built from source, but instead expose API to use prebuilt versions for use in your own build scripts.

RegEx-2.7 is a source build but is not yet mature. Download the [source](http://gnuwin32.sourceforge.net/packages/regex.htm).

## Tests
### FlexTest1
Generates a scanner to count the number of lines and characters for a file redirected into stdin.

This demonstrates the use of Flex build scripts, and the options that can be passed to the flex tool.

The test builds on Windows, Linux and OSX in all build modes.

## Platform specifics
### Windows
Prebuilt binaries can be downloaded from 
* [flex 2.5.4a](http://gnuwin32.sourceforge.net/packages/flex.htm)
* [bison 2.4.1](http://gnuwin32.sourceforge.net/packages/bison.htm)

Recommend downloading the zip files, and extracting these into the flex-2.x and bison-2.x package directories.
Note that the bison download also requires the 'Dependencies' zip file in order to run.

Note that the library for Flex (libfl.a) is not compatible with VisualStudio builds. Since this only contains main and yywrap functions, use %option noyywrap in the Flex source to avoid the need to link.

### Linux
Prebuilt binaries can be installed from package managers.
Note that on Ubuntu 14, bison 3.0.2 is installed by default, and under testing, was not working as expected. To install an older bison, please follow [the instructions in the accepted answer](http://askubuntu.com/questions/444982/install-bison-2-7-in-ubuntu-14-04).

### OSX
Flex and bison are installed as part of the Xcode command line tools.
