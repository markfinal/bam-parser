using Bam.Core;
namespace bison
{
    public class BisonGeneratedSource :
        C.SourceFile
    {
        private C.HeaderFile SourceHeaderModule;
        private IBisonGenerationPolicy Policy = null;

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<BisonTool>();
            this.Requires(this.Compiler);
        }

        public C.HeaderFile SourceHeader
        {
            get
            {
                return this.SourceHeaderModule;
            }
            set
            {
                this.SourceHeaderModule = value;
                this.DependsOn(value);
                this.GeneratedPaths[Key].Aliased(this.CreateTokenizedString("$(encapsulatingbuilddir)/$(config)/@changeextension(@trimstart(@relativeto($(0),$(packagedir)),../),.cpp)", value.GeneratedPaths[C.HeaderFile.Key]));
                this.GetEncapsulatingReferencedModule(); // or the path above won't be parsable prior to all modules having been created
            }
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var generatedPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(generatedPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            var sourceFileWriteTime = System.IO.File.GetLastWriteTime(generatedPath);
            var headerFileWriteTime = System.IO.File.GetLastWriteTime(this.SourceHeaderModule.InputPath.Parse());
            if (headerFileWriteTime > sourceFileWriteTime)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.SourceHeaderModule.InputPath);
                return;
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            this.Policy.Bison(this, context, this.Compiler, this.GeneratedPaths[Key], this.SourceHeader);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "bison." + mode + "BisonGeneration";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<IBisonGenerationPolicy>.Create(className);
        }

        private Bam.Core.PreBuiltTool Compiler
        {
            get
            {
                return this.Tool as Bam.Core.PreBuiltTool;
            }

            set
            {
                this.Tool = value;
            }
        }
    }
}
