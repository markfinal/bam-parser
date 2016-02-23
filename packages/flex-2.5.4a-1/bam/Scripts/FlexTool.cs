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
    class FlexTool :
        Bam.Core.PreBuiltTool
    {
        private Bam.Core.TokenizedStringArray arguments = new Bam.Core.TokenizedStringArray();

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
                this.arguments.Add(Bam.Core.TokenizedString.CreateVerbatim(System.String.Format("--sdk {0}", clangMeta.SDK)));
                this.arguments.Add("flex");
            }
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(T module)
        {
            return new FlexSettings(module);
        }

        public override TokenizedString Executable
        {
            get
            {
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return this.CreateTokenizedString("$(packagedir)/bin/flex.exe");
                }
                else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                {
                    return Bam.Core.TokenizedString.CreateVerbatim("xcrun");
                }
                else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                {
                    var flexLocation = GccCommon.ConfigureUtilities.RunExecutable("which", "flex");
                    if (null == flexLocation)
                    {
                        throw new Bam.Core.Exception("flex could not be found");
                    }
                    return Bam.Core.TokenizedString.CreateVerbatim(flexLocation);
                }

                throw new Bam.Core.Exception("flex unsupported on this platform");
            }
        }

        public override TokenizedStringArray InitialArguments
        {
            get
            {
                return this.arguments;
            }
        }
    }
}
