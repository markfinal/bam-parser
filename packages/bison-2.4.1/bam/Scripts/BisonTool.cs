using Bam.Core;
namespace bison
{
    class BisonTool :
        Bam.Core.PreBuiltTool
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                this.EnvironmentVariables.Add("PATH", new TokenizedStringArray(this.CreateTokenizedString("$(packagedir)/bin")));
            }
        }

        public override Bam.Core.Settings
        CreateDefaultSettings<T>(T module)
        {
            return new BisonSettings(module);
        }

        public override TokenizedString Executable
        {
            get
            {
                if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return this.CreateTokenizedString("$(packagedir)/bin/bison.exe");
                }

                throw new Bam.Core.Exception("bison unsupported on this platform");
            }
        }
    }
}
