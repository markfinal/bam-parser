using Bam.Core;
namespace flex.FlexExtension
{
    public static class FlexExtension
    {
        public static System.Tuple<Bam.Core.Module, Bam.Core.Module>
        FlexHeader(
            this C.Cxx.ObjectFileCollection collection,
            C.HeaderFile header)
        {
            // flex the header file to generate the source file
            var flexSourceFile = Bam.Core.Module.Create<FlexGeneratedSource>(collection);

            // compile the generated source file
            var objFile = collection.AddFile(flexSourceFile);

            // set the source header AFTER the source has been chained into the object file
            // so that the encapsulating module can be determined
            flexSourceFile.SourceHeader = header;

            // return both flex'd source, and the compiled object file
            return new System.Tuple<Bam.Core.Module, Bam.Core.Module>(flexSourceFile, objFile);
        }
    }
}
