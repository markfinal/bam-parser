using Bam.Core;
namespace bison
{
    public sealed class NativeBisonGeneration :
        IBisonGenerationPolicy
    {
        void
        IBisonGenerationPolicy.Bison(
            BisonGeneratedSource sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool bisonCompiler,
            Bam.Core.TokenizedString generatedFlexSource,
            C.HeaderFile source)
        {
            var bisonOutputPath = generatedFlexSource.Parse();
            var bisonOutputDir = System.IO.Path.GetDirectoryName(bisonOutputPath);
            if (!System.IO.Directory.Exists(bisonOutputDir))
            {
                System.IO.Directory.CreateDirectory(bisonOutputDir);
            }

            var args = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(args);
            args.Add(System.String.Format("-o{0}", bisonOutputPath));
            args.Add(source.InputPath.Parse());
            CommandLineProcessor.Processor.Execute(context, bisonCompiler, args);
        }
    }
}
