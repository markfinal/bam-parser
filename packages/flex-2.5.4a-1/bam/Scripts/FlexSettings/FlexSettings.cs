using Bam.Core;
namespace flex
{
    public sealed class FlexSettings :
        Bam.Core.Settings,
        CommandLineProcessor.IConvertToCommandLine,
        IFlexSettings
    {
        public FlexSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, true, true);
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.StringArray commandLine)
        {
            (this as IFlexSettings).Convert(commandLine);
        }
    }
}
