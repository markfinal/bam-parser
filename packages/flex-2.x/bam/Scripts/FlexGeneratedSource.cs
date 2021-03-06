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
namespace flex
{
    class FlexGeneratedSource :
        C.SourceFile,
        Bam.Core.ICloneModule
    {
        private FlexSourceFile SourceModule;

        protected override void
        Init()
        {
            base.Init();

            var graph = Bam.Core.Graph.Instance;
            this.Compiler = graph.FindReferencedModule<FlexTool>();
            this.Requires(this.Compiler);

            var parentModule = graph.ModuleStack.Peek();
            this.InputPath = this.CreateTokenizedString(
                "$(0)/$(1)/$(config)/@isrelative(@dir(@trimstart(@relativeto($(FlexSource),$(packagedir)),../)),.)/lex.@changeextension(#valid($(FlexModuleName),@basename($(FlexSource))),.cpp)",
                parentModule.Macros[Bam.Core.ModuleMacroNames.PackageBuildDirectory],
                parentModule.Macros[Bam.Core.ModuleMacroNames.ModuleName]
            );
        }

        public Bam.Core.TokenizedString ModuleName
        {
            get
            {
                return this.Macros["FlexModuleName"];
            }
            set
            {
                this.Macros.Add("FlexModuleName", value);
            }
        }

        public FlexSourceFile Source
        {
            get
            {
                return this.SourceModule;
            }
            set
            {
                this.SourceModule = value;
                this.DependsOn(value);
                this.Macros.Add("FlexSource", value.InputPath);
            }
        }

        protected override void
        EvaluateInternal()
        {
            this.ReasonToExecute = null;
            var generatedPath = this.GeneratedPaths[SourceFileKey].ToString();
            if (!System.IO.File.Exists(generatedPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(
                    this.GeneratedPaths[SourceFileKey]
                );
                return;
            }
            var generatedFileWriteTime = System.IO.File.GetLastWriteTime(generatedPath);
            var sourceFileWriteTime = System.IO.File.GetLastWriteTime(this.SourceModule.InputPath.ToString());
            if (sourceFileWriteTime > generatedFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                    this.GeneratedPaths[SourceFileKey],
                    this.SourceModule.InputPath
                );
                return;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.Add(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeBuilder.Support.RunCommandLineTool(this, context);
                    break;
#endif

#if D_PACKAGE_VSSOLUTIONBUILDER
                case "VSSolution":
                    VSSolutionBuilder.Support.AddCustomBuildStepForCommandLineTool(
                        this,
                        this.GeneratedPaths[SourceFileKey],
                        "Flex'ing",
                        true
                    );
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    {
                        XcodeBuilder.Support.AddPreBuildStepForCommandLineTool(
                            this,
                            out XcodeBuilder.Target target,
                            out XcodeBuilder.Configuration configuration,
                            XcodeBuilder.FileReference.EFileType.LexFile,
                            true,
                            false,
                            outputPaths: new Bam.Core.TokenizedStringArray(this.GeneratedPaths[SourceFileKey])
                        );
                    }
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
        }

        private Bam.Core.PreBuiltTool Compiler
        {
            get
            {
                return this.Tool as Bam.Core.PreBuiltTool;
            }

            set
            {
                this.Tool = value;
            }
        }

        public Bam.Core.TokenizedString LibrarySearchPath
        {
            get
            {
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return this.Compiler.Macros["LibraryPath"];
                }
                return null; // indicates system library
            }
        }

        public string StandardLibrary(
            C.LinkerTool linker)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                if (linker is VisualCCommon.LinkerBase)
                {
                    return string.Empty; // the library provided does not link with VisualC, use %option noyywrap
                }
                else
                {
                    return "-lfl";
                }
            }
            else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                return "-ll"; // note libl.a not libfl.a
            }
            return "-lfl";
        }

        Bam.Core.Module
        ICloneModule.Clone(
            Bam.Core.Module parent,
            Bam.Core.Module.PostInitDelegate postInitCB)
        {
            var clone = Bam.Core.Module.CloneWithPrivatePatches<FlexGeneratedSource>(
                this,
                parent,
                postInitCallback: postInitCB
            );
            clone.ModuleName = this.ModuleName;
            clone.Source = this.Source;
            return clone;
        }

        public override System.Collections.Generic.IEnumerable<(Bam.Core.Module module, string pathKey)> InputModulePaths
        {
            get
            {
                yield return (this.SourceModule, FlexSourceFile.FlexSourceKey);
            }
        }
    }
}
