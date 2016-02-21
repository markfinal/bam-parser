using Bam.Core;
namespace flex
{
    public sealed class NativeFlexGeneration :
        IFlexGenerationPolicy
    {
        void
        IFlexGenerationPolicy.Flex(
            FlexGeneratedSource sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool flexCompiler,
            Bam.Core.TokenizedString generatedFlexSource,
            C.HeaderFile source)
        {
            var flexOutputPath = generatedFlexSource.Parse();
            var flexOutputDir = System.IO.Path.GetDirectoryName(flexOutputPath);
            if (!System.IO.Directory.Exists(flexOutputDir))
            {
                System.IO.Directory.CreateDirectory(flexOutputDir);
            }

            var args = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(args);
            args.Add(System.String.Format("-o{0}", flexOutputPath));
            args.Add(source.InputPath.Parse());
            CommandLineProcessor.Processor.Execute(context, flexCompiler, args);
        }
    }
}
