using Bam.Core;
namespace bison
{
    [ModuleGroup("Thirdparty/bison")]
    class GenerateConfHeader :
        C.ProceduralHeaderFile
    {
        protected override TokenizedString OutputPath
        {
            get
            {
                return this.CreateTokenizedString("$(packagebuilddir)/PrivateHeaders/config.h");
            }
        }

        protected override string Contents
        {
            get
            {
                var contents = new System.Text.StringBuilder();
                contents.AppendLine("#define HAVE_DECL_CLEARERR_UNLOCKED 1");
                contents.AppendLine("#define HAVE_DECL_FEOF_UNLOCKED 1");
                contents.AppendLine("#define HAVE_DECL_FERROR_UNLOCKED 1");
                contents.AppendLine("#define HAVE_DECL_GETC_UNLOCKED 1");
                contents.AppendLine("#define HAVE_DECL_GETCHAR_UNLOCKED 1");
                contents.AppendLine("#define HAVE_DECL_PUTC_UNLOCKED 1");
                contents.AppendLine("#define HAVE_DECL_PUTCHAR_UNLOCKED 1");
                contents.AppendLine("#define _GL_INLINE_HEADER_BEGIN");
                contents.AppendLine("#define _GL_INLINE_HEADER_END");
                contents.AppendLine("#define _GL_INLINE static"); // TODO
                contents.AppendLine("#define _GL_ATTRIBUTE_PURE");
                contents.AppendLine("#define _GL_ATTRIBUTE_FORMAT_PRINTF(_a,_b)");
                contents.AppendLine("#define _GL_ATTRIBUTE_CONST");
                contents.AppendLine("#define _GL_EXTERN_INLINE");
                contents.AppendLine("#define _GL_UNUSED");
                contents.AppendLine("#define HAVE_DECL_OBSTACK_PRINTF 0");
                contents.AppendLine("#define HAVE_OBSTACK_PRINTF 0");
                contents.AppendLine("#define HAVE_DECL_STRERROR_R 1");
                contents.AppendLine("struct obstack *obs;");
                contents.AppendLine("extern int obstack_printf (struct obstack *obs, const char *format, ...);");
                contents.AppendLine("#include <stdarg.h>");
                contents.AppendLine("extern int obstack_vprintf (struct obstack *obs, const char *format, va_list args);");
                contents.AppendLine("#include <stdio.h>");
                contents.AppendLine("extern FILE *fopen_safer (char const *file, char const *mode);");
                contents.AppendLine("#define PACKAGE_NAME \"GNU Bison\"");
                contents.AppendLine("#define PACKAGE_URL \"http://www.gnu.org/software/bison/\"");
                contents.AppendLine("#define PACKAGE_COPYRIGHT_YEAR 2015");
                contents.AppendLine("#define PACKAGE_BUGREPORT \"bug-bison@gnu.org\"");
                contents.AppendLine("#define PACKAGE_STRING \"GNU Bison 3.0.4\"");
                contents.AppendLine("#define PACKAGE_VERSION \"3.0.4\"");
                contents.AppendLine("#define PACKAGE \"bison\"");
                contents.AppendLine("#define VERSION \"3.0.4\"");
                contents.AppendLine("#define M4 \"m4\"");
                contents.AppendLine("#define M4_GNU_OPTION \"\"");
                contents.AppendLine("extern int strverscmp (const char *s1, const char *s2);");
                contents.AppendLine("#define O_BINARY 0"); // not defined on *nix fcntl.h
                contents.AppendLine("#define O_TEXT 0"); // not defined on *nix fcntl.h
                //contents.AppendLine("#define _LIBC 1"); // TODO: windows?
                contents.AppendLine("#ifndef _GL_ARG_NONNULL");
                contents.AppendLine("#define _GL_ARG_NONNULL(_a)");
                contents.AppendLine("#endif");
                contents.AppendLine("#define HAVE_FCNTL 1");
                contents.AppendLine("#define GNULIB_defined_F_DUPFD_CLOEXEC 0");
                contents.AppendLine("#define GNULIB_FD_SAFER_FLAG 1");
                //contents.AppendLine("#define GNULIB_PIPE2_SAFER 1");
                contents.AppendLine("#define PENDING_OUTPUT_N_BYTES 16"); // TODO: arbitrary
                contents.AppendLine("#define __getopt_argv_const");
                contents.AppendLine("#define OK_TO_USE_1S_CLOCK");
                contents.AppendLine("#define isnanl(x) isnan ((long double)(x))");
                contents.AppendLine("#define HAVE_WORKING_O_NOFOLLOW 0");
                contents.AppendLine("#define PROMOTED_MODE_T int");
                contents.AppendLine("extern int pipe2 (int fd[2], int flags);");
                contents.AppendLine("extern char **environ;");
                return contents.ToString();
            }
        }
    }

    [ModuleGroup("Thirdparty/bison")]
    class GenerateConfigMakeHeader :
        C.ProceduralHeaderFile
    {
        protected override TokenizedString OutputPath
        {
            get
            {
                return this.CreateTokenizedString("$(packagebuilddir)/PrivateHeaders/configmake.h");
            }
        }

        protected override string Contents
        {
            get
            {
                var contents = new System.Text.StringBuilder();
                contents.AppendLine("#define LOCALEDIR \"\"");
                contents.AppendLine("#define PKGDATADIR \"\"");
                contents.AppendLine("#define LIBDIR \"\"");
                return contents.ToString();
            }
        }
    }

    [ModuleGroup("Thirdparty/bison")]
    class libbison :
        C.StaticLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(packagedir)/lib/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*getopt1)(?!.*msvc-nothrow)(?!.*sig)(?!.*snprintf)(?!.*spawn)(?!.*sprintf)(?!.*stpcpy)(?!.*strchrnul)(?!.*strerror-override)(?!.*strerror_r)(?!.*waitpid)(?!.*wcwidth)(?!.*xsize).*)$"));
            source.AddFiles("$(packagedir)/lib/fatal-signal.c");
            source.AddFiles("$(packagedir)/lib/spawn-pipe.c");
            source.AddFiles("$(packagedir)/lib/vasnprintf.c");

            var generateConfig = Graph.Instance.FindReferencedModule<GenerateConfHeader>();
            source.DependsOn(generateConfig);
            source.UsePublicPatches(generateConfig);

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/lib")); // not sure why this is necessary

                    var cCompiler = settings as C.ICOnlyCompilerSettings;
                    cCompiler.LanguageStandard = C.ELanguageStandard.C99;

                    if (this.Librarian is ClangCommon.Librarian)
                    {
                        compiler.DisableWarnings.AddUnique("format-extra-args");
                    }
                });
        }
    }

    [ModuleGroup("Thirdparty/bison")]
    sealed class bison :
        C.ConsoleApplication
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/src/*.h");
            var source = this.CreateCSourceContainer("$(packagedir)/src/*.c", filter: new System.Text.RegularExpressions.Regex(@"^((?!.*scan-code)(?!.*scan-gram)(?!.*scan-skel).*)$"));
            source.AddFiles("$(packagedir)/src/scan-code-c.c");
            source.AddFiles("$(packagedir)/src/scan-gram-c.c");
            source.AddFiles("$(packagedir)/src/scan-skel-c.c");

            var generateConfig = Graph.Instance.FindReferencedModule<GenerateConfHeader>();
            var generateConfigMake = Graph.Instance.FindReferencedModule<GenerateConfigMakeHeader>();
            source.DependsOn(generateConfig, generateConfigMake);
            source.UsePublicPatches(generateConfig);

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)")); // maybe only for src/scan-c.c
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/lib"));

                    var cCompiler = settings as C.ICOnlyCompilerSettings;
                    cCompiler.LanguageStandard = C.ELanguageStandard.C99;
                });

            this.CompileAndLinkAgainst<libbison>(source);
        }
    }
}
