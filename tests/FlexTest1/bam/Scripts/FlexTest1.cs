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

            var enableDebugging = true;

            var flexOutput = source.RunFlex(flexInput, Bam.Core.TokenizedString.CreateVerbatim("yy"), true);
            flexOutput.Item1.PrivatePatch(settings =>
                {
                    var flexSettings = settings as flex.IFlexSettings;
                    flexSettings.Debug = enableDebugging;
                    flexSettings.InsertLineDirectives = !enableDebugging;
                });
            flexOutput.Item2.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("D_SCANNER");
                });

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.ICommonLinkerSettings;
                    if (null != flexInput.LibrarySearchPath)
                    {
                        linker.LibraryPaths.AddUnique(flexInput.LibrarySearchPath);
                    }
                    linker.Libraries.AddUnique(flexInput.StandardLibrary);
                });
        }
    }
}
