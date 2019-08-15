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
namespace flex.FlexExtension
{
    public static class FlexExtension
    {
        public static System.Tuple<FlexGeneratedSource, C.Cxx.ObjectFile>
        RunFlex(
            this C.Cxx.ObjectFileCollection collection,
            flex.FlexSourceFile source,
            Bam.Core.TokenizedString moduleName,
            bool compile)
        {
            // flex the flex source file to generate the C++ source file
            var flexSourceFile = Bam.Core.Module.Create<FlexGeneratedSource>(collection);

            // compile the generated C++ source file
            var objFile = collection.AddFile(flexSourceFile);
            objFile.PerformCompilation = compile;

            collection.PrivatePatch(settings =>
                {
                    var preprocessor = settings as C.ICommonPreprocessorSettings;
                    preprocessor.IncludePaths.AddUnique(
                        collection.CreateTokenizedString(
                            "@dir($(0))",
                            objFile.GeneratedPaths[C.ObjectFile.ObjectFileKey]
                        )
                    );
                });

            if (collection.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                // for unistd.h
                var flexTool = Bam.Core.Graph.Instance.FindReferencedModule<flex.FlexTool>();
                collection.PrivatePatch(settings =>
                    {
                        var preprocessor = settings as C.ICommonPreprocessorSettings;
                        preprocessor.IncludePaths.AddUnique(
                            collection.CreateTokenizedString(
                                "$(0)/include",
                                flexTool.Macros[Bam.Core.ModuleMacroNames.PackageDirectory]
                            )
                        );
                    });
            }

            // override the name of the output file (if null, use the source .l basename)
            if (null != moduleName)
            {
                flexSourceFile.ModuleName = moduleName;
            }

            // set the Flex source AFTER the generated C++ source file has been chained into the object file
            // so that the encapsulating module can be determined
            flexSourceFile.Source = source;

            // return both generated C++ source, and the compiled object file
            return System.Tuple.Create(flexSourceFile, objFile);
        }
    }
}
