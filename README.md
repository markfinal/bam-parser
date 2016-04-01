# [![BuildAMation](http://buildamation.com/BAM-small.png)](https://github.com/markfinal/BuildAMation) parser package repository

This repository only contains the Bam build scripts for
* flex-2.x
* bison-2.x
* regex-2.7

Flex and Bison do not build from source, but use prebuilt versions. The build scripts expose running the tools to other build scripts.

RegEx-2.7 is a source build but is not yet mature. Download the [source](http://gnuwin32.sourceforge.net/packages/regex.htm).

## Windows
Prebuilt binaries can be downloaded from 
* [flex 2.5.4a](http://gnuwin32.sourceforge.net/packages/flex.htm)
* [bison 2.4.1](http://gnuwin32.sourceforge.net/packages/bison.htm)

Extract these into the flex-2.x and bison-2.x package directories.

## Linux
Prebuilt binaries can be installed from package managers.
Note that on Ubuntu 14, bison 3.0.2 is installed by default, and under testing, was not working as expected. To install an older bison, please follow [the instructions in the accepted answer](http://askubuntu.com/questions/444982/install-bison-2-7-in-ubuntu-14-04).

## OSX
Flex and bison are installed as part of the Xcode command line tools.
