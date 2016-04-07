using Bam.Core;
using flex.FlexExtension; // for extension methods
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

            var source = this.CreateCxxSourceContainer();

            var flexInput = Bam.Core.Module.Create<flex.FlexSourceFile>();
            flexInput.InputPath = this.CreateTokenizedString("$(packagedir)/source/scanner.l");

            var enableDebugging = false;

            var flexOutput = source.RunFlex(flexInput, Bam.Core.TokenizedString.CreateVerbatim("yy"), true);
            var flexGeneratedSource = flexOutput.Item1; // the .cpp file
            var flexCompiledGeneratedSource = flexOutput.Item2; // the .obj file
            flexGeneratedSource.PrivatePatch(settings =>
                {
                    var flexSettings = settings as flex.IFlexSettings;
                    flexSettings.Debug = enableDebugging;
                    flexSettings.InsertLineDirectives = !enableDebugging;
                });
            flexCompiledGeneratedSource.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("D_SCANNER");

                    if (this.Linker is VisualCCommon.LinkerBase)
                    {
                        compiler.DisableWarnings.AddUnique("4273"); // lex.yy.cpp(1200): warning C4273: 'isatty' : inconsistent dll linkage
                    }
                    else if (this.Linker is MingwCommon.LinkerBase)
                    {
                        var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                        cxxCompiler.LanguageStandard = C.Cxx.ELanguageStandard.GnuCxx98; // needed for fileno
                    }
                });

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
