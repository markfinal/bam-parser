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
namespace flex
{
#if BAM_V2
    public static partial class XcodeSupport
    {
        public static void
        Flex(
            FlexGeneratedSource module)
        {
            var encapsulating = module.GetEncapsulatingReferencedModule();

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(encapsulating);
            var configuration = target.GetConfiguration(encapsulating);

            var commands = new Bam.Core.StringArray();
            foreach (var dir in module.OutputDirectories)
            {
                commands.Add(
                    System.String.Format(
                        "[[ ! -d {0} ]] && mkdir -p {0}",
                        Bam.Core.IOWrapper.EscapeSpacesInPath(dir.ToString())
                    )
                );
            }

            var args = new Bam.Core.StringArray();
            args.Add(CommandLineProcessor.Processor.StringifyTool(module.Tool as Bam.Core.ICommandLineTool));
            args.AddRange(
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                )
            );

            var flex_commandLine = args.ToString(' ');
            var flex_source_path = module.InputPath.ToString();
            var flex_output_path = module.GeneratedPaths[FlexGeneratedSource.SourceFileKey].ToString();
            commands.Add(
                System.String.Format(
                    "if [[ ! -e {0} || {1} -nt {0} ]]",
                    Bam.Core.IOWrapper.EscapeSpacesInPath(flex_output_path),
                    Bam.Core.IOWrapper.EscapeSpacesInPath(flex_source_path)
                )
            );
            commands.Add("then");
            commands.Add(System.String.Format("\techo {0}", flex_commandLine));
            commands.Add(System.String.Format("\t{0}", flex_commandLine));
            commands.Add("fi");

            target.AddPreBuildCommands(commands, configuration);

            target.EnsureFileOfTypeExists(
                module.Source.InputPath,
                XcodeBuilder.FileReference.EFileType.LexFile
            );
        }
    }
#else
    public sealed class XcodeFlexGeneration :
        IFlexGenerationPolicy
    {
        void
        IFlexGenerationPolicy.Flex(
            FlexGeneratedSource sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool flexCompiler,
            Bam.Core.TokenizedString generatedFlexSource,
            FlexSourceFile source)
        {
            var encapsulating = sender.GetEncapsulatingReferencedModule();

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(encapsulating);
            var configuration = target.GetConfiguration(encapsulating);

            var output = generatedFlexSource.ToString();
            var sourcePath = source.InputPath.ToString();

            var commands = new Bam.Core.StringArray();
            commands.Add(
                System.String.Format(
                    "[[ ! -d {0} ]] && mkdir -p {0}",
                    Bam.Core.IOWrapper.EscapeSpacesInPath(System.IO.Path.GetDirectoryName(output))
                )
            );

            var args = new Bam.Core.StringArray();
            args.Add(CommandLineProcessor.Processor.StringifyTool(flexCompiler));
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(args);
            args.Add(System.String.Format("-o{0}", Bam.Core.IOWrapper.EscapeSpacesInPath(output)));
            args.Add(Bam.Core.IOWrapper.EscapeSpacesInPath(sourcePath));

            var flex_commandLine = args.ToString(' ');

            commands.Add(
                System.String.Format(
                    "if [[ ! -e {0} || {1} -nt {0} ]]",
                    Bam.Core.IOWrapper.EscapeSpacesInPath(output),
                    Bam.Core.IOWrapper.EscapeSpacesInPath(sourcePath)
                )
            );
            commands.Add("then");
            commands.Add(System.String.Format("\techo {0}", flex_commandLine));
            commands.Add(System.String.Format("\t{0}", flex_commandLine));
            commands.Add("fi");

            target.AddPreBuildCommands(commands, configuration);

            target.EnsureFileOfTypeExists(source.InputPath, XcodeBuilder.FileReference.EFileType.LexFile);
        }
    }
#endif
}
