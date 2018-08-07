#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace bison
{
#if BAM_V2
    public static partial class XcodeSupport
    {
        public static void
        Bison(
            BisonGeneratedSource module)
        {
            XcodeBuilder.Target target;
            XcodeBuilder.Configuration configuration;
            XcodeBuilder.Support.AddPreBuildStepForCommandLineTool(
                module,
                out target,
                out configuration,
                XcodeBuilder.FileReference.EFileType.YaccFile,
                true,
                false
            );
        }
    }
#else
    public sealed class XcodeBisonGeneration :
        IBisonGenerationPolicy
    {
        void
        IBisonGenerationPolicy.Bison(
            BisonGeneratedSource sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool bisonCompiler,
            Bam.Core.TokenizedString generatedYaccSource,
            BisonSourceFile source)
        {
            var encapsulating = sender.GetEncapsulatingReferencedModule();

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(encapsulating);
            var configuration = target.GetConfiguration(encapsulating);

            var output = generatedYaccSource.ToString();
            var sourcePath = source.InputPath.ToString();

            var commands = new Bam.Core.StringArray();
            commands.Add(
                System.String.Format(
                    "[[ ! -d {0} ]] && mkdir -p {0}",
                    Bam.Core.IOWrapper.EscapeSpacesInPath(System.IO.Path.GetDirectoryName(output))
                )
            );

            var args = new Bam.Core.StringArray();
            args.Add(CommandLineProcessor.Processor.StringifyTool(bisonCompiler));
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(args);
            args.Add(System.String.Format("-o{0}", Bam.Core.IOWrapper.EscapeSpacesInPath(output)));
            args.Add(Bam.Core.IOWrapper.EscapeSpacesInPath(sourcePath));

            var bison_commandLine = args.ToString(' ');

            commands.Add(
                System.String.Format(
                    "if [[ ! -e {0} || {1} -nt {0} ]]",
                    Bam.Core.IOWrapper.EscapeSpacesInPath(output),
                    Bam.Core.IOWrapper.EscapeSpacesInPath(sourcePath)
                )
            );
            commands.Add("then");
            commands.Add(System.String.Format("\techo {0}", bison_commandLine));
            commands.Add(System.String.Format("\t{0}", bison_commandLine));
            commands.Add("fi");

            target.AddPreBuildCommands(commands, configuration);

            target.EnsureFileOfTypeExists(source.InputPath, XcodeBuilder.FileReference.EFileType.YaccFile);
        }
    }
#endif
}
