using Bam.Core;
namespace bison
{
    public sealed class BisonSettings :
        Bam.Core.Settings,
        CommandLineProcessor.IConvertToCommandLine,
        IBisonSettings
    {
        public BisonSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, true, true);
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.StringArray commandLine)
        {
            (this as IBisonSettings).Convert(commandLine);
        }
    }
}
