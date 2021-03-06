using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HandlebarsDotNet.Features
{
    internal class DefaultCompilerFeatureFactory : IFeatureFactory
    {
        public IFeature CreateFeature() => new DefaultCompilerFeature();
    }

    [FeatureOrder(2)]
    internal class DefaultCompilerFeature : IFeature
    {
        public void OnCompiling(ICompiledHandlebarsConfiguration configuration)
        {
            configuration.ExpressionCompiler = new DefaultExpressionCompiler(configuration);
        }

        public void CompilationCompleted()
        {
            // noting to do here
        }

        private class DefaultExpressionCompiler : IExpressionCompiler
        {
            private readonly IReadOnlyList<IExpressionMiddleware> _expressionMiddleware;
            
            public DefaultExpressionCompiler(ICompiledHandlebarsConfiguration configuration)
            {
                _expressionMiddleware = configuration.ExpressionMiddlewares;
            }
            
            public T Compile<T>(Expression<T> expression) where T: class, Delegate
            {
                for (var index = 0; index < _expressionMiddleware.Count; index++)
                {
                    expression = _expressionMiddleware[index].Invoke(expression);
                }

                return expression.Compile();
            }
        }
    }
}