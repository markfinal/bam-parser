#region License
// Copyright (c) 2010-2017, Mark Final
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
    public sealed class VSSolutionBisonGeneration :
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

            var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
            var project = solution.EnsureProjectExists(encapsulating);
            var config = project.GetConfiguration(encapsulating);

            var output = generatedYaccSource.ToString();

            var commands = new Bam.Core.StringArray();
            var dir = sender.CreateTokenizedString("@dir($(0))", generatedYaccSource);
            dir.Parse();
            commands.Add(System.String.Format("IF NOT EXIST {0} MKDIR {0}", dir.ToString()));

            var args = new Bam.Core.StringArray();
            foreach (var envVar in bisonCompiler.EnvironmentVariables)
            {
                args.Add("set");
                var content = new System.Text.StringBuilder();
                content.AppendFormat("{0}=", envVar.Key);
                foreach (var value in envVar.Value)
                {
                    content.AppendFormat("{0};", value.ToStringQuoteIfNecessary());
                }
                content.AppendFormat("%{0}%", envVar.Key);
                args.Add(content.ToString());
                args.Add("&&");
            }
            args.Add(CommandLineProcessor.Processor.StringifyTool(bisonCompiler));
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(args);
            args.Add(System.String.Format("-o{0}", output));
            args.Add("%(FullPath)");
            commands.Add(args.ToString(' '));

            var customBuild = config.GetSettingsGroup(VSSolutionBuilder.VSSettingsGroup.ESettingsGroup.CustomBuild, include: source.InputPath, uniqueToProject: true);
            customBuild.AddSetting("Command", commands.ToString(System.Environment.NewLine), condition: config.ConditionText);
            customBuild.AddSetting("Message", System.String.Format("Yacc'ing {0} into {1}", System.IO.Path.GetFileName(source.InputPath.ToString()), output), condition: config.ConditionText);
            customBuild.AddSetting("Outputs", output, condition: config.ConditionText);

            config.AddOtherFile(source);
        }
    }
}
