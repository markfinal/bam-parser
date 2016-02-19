using Bam.Core;
namespace flex
{
    sealed class flex :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/src/*.h");
            var source = this.CreateCSourceContainer("$(packagedir)/src/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*libmain)(?!.*libyywrap).*)$"));
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("STDC_HEADERS");
                    compiler.PreprocessorDefines.Add("HAVE_REGEX_H");
                    compiler.PreprocessorDefines.Add("HAVE_LIMITS_H");
                    compiler.PreprocessorDefines.Add("VERSION", "\"2.6.0\"");
                    compiler.PreprocessorDefines.Add("M4", "m4");
                });
        }
    }
}
