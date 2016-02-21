using Bam.Core;
namespace bison.BisonExtension
{
    public static class BisonExtension
    {
        public static System.Tuple<Bam.Core.Module, Bam.Core.Module>
        BisonHeader(
            this C.Cxx.ObjectFileCollection collection,
            C.HeaderFile header)
        {
            // bison the header file to generate the source file
            var bisonSourceFile = Bam.Core.Module.Create<BisonGeneratedSource>(collection);

            // compile the generated source file
            var objFile = collection.AddFile(bisonSourceFile);

            // set the source header AFTER the source has been chained into the object file
            // so that the encapsulating module can be determined
            bisonSourceFile.SourceHeader = header;

            // return both bison'd source, and the compiled object file
            return new System.Tuple<Bam.Core.Module, Bam.Core.Module>(bisonSourceFile, objFile);
        }
    }
}
