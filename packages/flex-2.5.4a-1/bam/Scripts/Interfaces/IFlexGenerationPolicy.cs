using Bam.Core;
namespace flex
{
    public interface IFlexGenerationPolicy
    {
        void
        Flex(
            FlexGeneratedSource sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.ICommandLineTool flexTool,
            Bam.Core.TokenizedString generatedFlexSource,
            C.HeaderFile source); // TODO: needs to be a .l file
    }
}
