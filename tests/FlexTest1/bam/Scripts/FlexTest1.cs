#region License
// Copyright (c) 2010-2019, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using Bam.Core;
using flex.FlexExtension; // for extension methods to add the compiled flex generated source into a C++ source container
namespace FlexTest1
{
    sealed class FlexTest :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // specify the input .l file
            var flexInput = Bam.Core.Module.Create<flex.FlexSourceFile>();
            flexInput.InputPath = this.CreateTokenizedString("$(packagedir)/source/scanner.l");

            var source = this.CreateCxxSourceContainer();
            // generate the .cpp (and .obj) from running flex, and add it to the container of sources for this app
            var flexOutput = source.RunFlex(flexInput, Bam.Core.TokenizedString.CreateVerbatim("yy"), true);

            // modify the settings used to generate the .cpp file, and to compile the .obj file
            var flexGeneratedSource = flexOutput.Item1; // the .cpp file
            var flexCompiledGeneratedSource = flexOutput.Item2; // the .obj file
            flexGeneratedSource.PrivatePatch(settings =>
                {
                    var enableDebugging = false;

                    var flexSettings = settings as flex.IFlexSettings;
                    flexSettings.Debug = enableDebugging;
                    flexSettings.InsertLineDirectives = !enableDebugging;
                });
            flexCompiledGeneratedSource.PrivatePatch(settings =>
                {
                    if (this.Linker is VisualCCommon.LinkerBase)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4273"); // lex.yy.cpp(1200): warning C4273: 'isatty' : inconsistent dll linkage
                    }
                    else if (this.Linker is MingwCommon.LinkerBase)
                    {
                        var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                        cxxCompiler.LanguageStandard = C.Cxx.ELanguageStandard.GnuCxx98; // needed for fileno
                    }
                });

            // may need to link against the flex library for yywrap
            this.PrivatePatch(settings =>
                {
                    // this is only necessary when %option noyywrap is missing, or yywrap is undefined
                    // VisualC is the exception, where the provided library with the prebuilt flex is incompatible, and %option noyywrap must be defined
                    var linker = settings as C.ICommonLinkerSettings;
                    if (null != flexGeneratedSource.LibrarySearchPath)
                    {
                        linker.LibraryPaths.AddUnique(flexGeneratedSource.LibrarySearchPath);
                    }
                    linker.Libraries.AddUnique(flexGeneratedSource.StandardLibrary(this.Linker));
                });
        }
    }
}
