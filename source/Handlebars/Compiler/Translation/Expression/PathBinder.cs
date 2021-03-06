using System.Linq.Expressions;
using Expressions.Shortcuts;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.Helpers;
using HandlebarsDotNet.Runtime;
using static Expressions.Shortcuts.ExpressionShortcuts;

namespace HandlebarsDotNet.Compiler
{
    internal class PathBinder : HandlebarsExpressionVisitor
    {
        private CompilationContext CompilationContext { get; }

        public PathBinder(CompilationContext compilationContext)
        {
            CompilationContext = compilationContext;
        }
        
        protected override Expression VisitStatementExpression(StatementExpression sex)
        {
            if (!(sex.Body is PathExpression)) return Visit(sex.Body);

            var writer = CompilationContext.Args.EncodedWriter;
            
            var value = Arg<object>(Visit(sex.Body));
            return writer.Call(o => o.Write<object>(value));
        }

        protected override Expression VisitPathExpression(PathExpression pex)
        {
            var bindingContext = CompilationContext.Args.BindingContext;
            var configuration = CompilationContext.Configuration;
            var pathInfo = PathInfoStore.Shared.GetOrAdd(pex.Path);

            var resolvePath = Call(() => PathResolver.ResolvePath(bindingContext, pathInfo));
            
            if (pex.Context == PathExpression.ResolutionContext.Parameter) return resolvePath;
            if (pathInfo.IsVariable || pathInfo.IsThis) return resolvePath;
            if (!pathInfo.IsValidHelperLiteral && !configuration.Compatibility.RelaxedHelperNaming) return resolvePath;

            var pathInfoLight = new PathInfoLight(pathInfo);
            if (!configuration.Helpers.TryGetValue(pathInfoLight, out var helper))
            {
                var lateBindHelperDescriptor = new LateBindHelperDescriptor(pathInfo);
                helper = new Ref<IHelperDescriptor<HelperOptions>>(lateBindHelperDescriptor);
                configuration.Helpers.AddOrReplace(pathInfoLight, helper);
            }
            else if (configuration.Compatibility.RelaxedHelperNaming)
            {
                pathInfoLight = pathInfoLight.TagComparer();
                if (!configuration.Helpers.ContainsKey(pathInfoLight))
                {
                    var lateBindHelperDescriptor = new LateBindHelperDescriptor(pathInfo);
                    helper = new Ref<IHelperDescriptor<HelperOptions>>(lateBindHelperDescriptor);
                    configuration.Helpers.AddOrReplace(pathInfoLight, helper);
                }
            }

            var options = New(() => new HelperOptions(bindingContext));
            var context = New(() => new Context(bindingContext));
            var argumentsArg = New(() => new Arguments(0));
            return Call(() => helper.Value.Invoke(options, context, argumentsArg));
        }
    }
}

