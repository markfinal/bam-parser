using Bam.Core;
namespace regex
{
    sealed class regex :
        C.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/src/regex/2.7/regex-2.7-src/src/*.h");
            var source = this.CreateCSourceContainer("$(packagedir)/src/regex/2.7/regex-2.7-src/src/regex.c");

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/regex/2.7/regex-2.7-src/src"));
                    }
                });

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("REGEX_STATIC");

                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.PreprocessorDefines.Add("_WIN32");
                        compiler.PreprocessorDefines.Add("HAVE_STDBOOL_H");
                    }
                });
        }
    }
}
