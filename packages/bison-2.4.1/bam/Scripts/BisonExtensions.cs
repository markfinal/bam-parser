#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace bison.BisonExtension
{
    public static class BisonExtension
    {
        public static System.Tuple<Bam.Core.Module, Bam.Core.Module>
        BisonHeader(
            this C.Cxx.ObjectFileCollection collection,
            C.HeaderFile header,
            Bam.Core.TokenizedString macroDefinitionHeaderPath = null)
        {
            // bison the header file to generate the source file
            var bisonSourceFile = Bam.Core.Module.Create<BisonGeneratedSource>(collection);

            // compile the generated source file
            var objFile = collection.AddFile(bisonSourceFile);

            // set the source header AFTER the source has been chained into the object file
            // so that the encapsulating module can be determined
            bisonSourceFile.SourceHeader = header;

            // on Linux, in C++11 mode, bison defines YY_NULL as nullptr, but flex does it as 0, causing an error
            // I think it's because bison is newer in Ubuntu (2.7, 3.0.2 is broken as default) than on the other platforms (2.5)
            if (objFile.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
            {
                objFile.PrivatePatch(settings =>
                    {
                        var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                        cxxCompiler.LanguageStandard = C.Cxx.ELanguageStandard.Cxx98;
                    });
            }

            // return both bison'd source, and the compiled object file
            return new System.Tuple<Bam.Core.Module, Bam.Core.Module>(bisonSourceFile, objFile);
        }
    }
}
