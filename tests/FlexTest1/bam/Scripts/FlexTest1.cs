using Bam.Core;
using flex.FlexExtension; // for extension methods to add the compiled flex generated source into a C++ source container
namespace FlexTest1
{
    sealed class FlexTest :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            // specify the input .l file
            var flexInput = Bam.Core.Module.Create<flex.FlexSourceFile>();
            flexInput.InputPath = this.CreateTokenizedString("$(packagedir)/source/scanner.l");

            var source = this.CreateCxxSourceContainer();
            // generate the .cpp (and .obj) from running flex, and add it to the container of sources for this app
            var flexOutput = source.RunFlex(flexInput, Bam.Core.TokenizedString.CreateVerbatim("yy"), true);

            // modify the settings used to generate the .cpp file, and to compile the .obj file
            var flexGeneratedSource = flexOutput.Item1; // the .cpp file
            var flexCompiledGeneratedSource = flexOutput.Item2; // the .obj file
            flexGeneratedSource.PrivatePatch(settings =>
                {
                    var enableDebugging = false;

                    var flexSettings = settings as flex.IFlexSettings;
                    flexSettings.Debug = enableDebugging;
                    flexSettings.InsertLineDirectives = !enableDebugging;
                });
            flexCompiledGeneratedSource.PrivatePatch(settings =>
                {
                    if (this.Linker is VisualCCommon.LinkerBase)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.AddUnique("4273"); // lex.yy.cpp(1200): warning C4273: 'isatty' : inconsistent dll linkage
                    }
                    else if (this.Linker is MingwCommon.LinkerBase)
                    {
                        var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                        cxxCompiler.LanguageStandard = C.Cxx.ELanguageStandard.GnuCxx98; // needed for fileno
                    }
                });

            // may need to link against the flex library for yywrap
            this.PrivatePatch(settings =>
                {
                    // this is only necessary when %option noyywrap is missing, or yywrap is undefined
                    // VisualC is the exception, where the provided library with the prebuilt flex is incompatible, and %option noyywrap must be defined
                    var linker = settings as C.ICommonLinkerSettings;
                    if (null != flexGeneratedSource.LibrarySearchPath)
                    {
                        linker.LibraryPaths.AddUnique(flexGeneratedSource.LibrarySearchPath);
                    }
                    linker.Libraries.AddUnique(flexGeneratedSource.StandardLibrary(this.Linker));
                });

            if (this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }
}
