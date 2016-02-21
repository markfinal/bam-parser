using Bam.Core;
namespace flex
{
    class FlexTool :
        Bam.Core.PreBuiltTool
    {
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

                throw new Bam.Core.Exception("flex unsupported on this platform");
            }
        }
    }
}
