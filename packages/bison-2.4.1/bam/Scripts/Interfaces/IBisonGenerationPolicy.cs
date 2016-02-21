using Bam.Core;
namespace bison
{
    public interface IBisonGenerationPolicy
    {
        void
        Bison(
            BisonGeneratedSource sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool bisonTool,
            Bam.Core.TokenizedString generatedBisonSource,
            C.HeaderFile source); // TODO: needs to be a .y file
    }
}
