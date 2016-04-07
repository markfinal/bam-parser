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
namespace flex
{
    public class FlexGeneratedSource :
        C.SourceFile
    {
        private FlexSourceFile SourceModule;
        private IFlexGenerationPolicy Policy = null;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<FlexTool>();
            this.Requires(this.Compiler);
        }

        public Bam.Core.TokenizedString ModuleName
        {
            get;
            set;
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
                if (null != this.ModuleName)
                {
                    this.GeneratedPaths[Key].Aliased(this.CreateTokenizedString("$(encapsulatingbuilddir)/$(encapsulatedparentmodulename)/$(config)/@dir(@trimstart(@relativeto($(0),$(packagedir)),../))/lex.$(1).cpp", value.GeneratedPaths[FlexSourceFile.Key], this.ModuleName));
                }
                else
                {
                    this.GeneratedPaths[Key].Aliased(this.CreateTokenizedString("$(encapsulatingbuilddir)/$(encapsulatedparentmodulename)/$(config)/@dir(@trimstart(@relativeto($(0),$(packagedir)),../))/lex.@changeextension(@basename($(0)),.cpp)", value.GeneratedPaths[FlexSourceFile.Key]));
                }
                this.GetEncapsulatingReferencedModule(); // or the path above won't be parsable prior to all modules having been created
            }
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var generatedPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(generatedPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            var generatedFileWriteTime = System.IO.File.GetLastWriteTime(generatedPath);
            var sourceFileWriteTime = System.IO.File.GetLastWriteTime(this.SourceModule.InputPath.Parse());
            if (sourceFileWriteTime > generatedFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.SourceModule.InputPath);
                return;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            this.Policy.Flex(this, context, this.Compiler, this.GeneratedPaths[Key], this.SourceModule);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "flex." + mode + "FlexGeneration";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<IFlexGenerationPolicy>.Create(className);
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
    }
}
