using HandlebarsDotNet.Compiler.Structure.Path;

namespace HandlebarsDotNet.Helpers.BlockHelpers
{
    internal sealed class InlineBlockHelperDescriptor : IHelperDescriptor<BlockHelperOptions>
    {
        public PathInfo Name { get; } = "*inline";

        public object Invoke(in BlockHelperOptions options, in Context context, in Arguments arguments)
        {
            return this.ReturnInvoke(options, context, arguments);
        }

        public void Invoke(in EncodedTextWriter output, in BlockHelperOptions options, in Context context, in Arguments arguments)
        {
            if (arguments.Length != 1)
            {
                throw new HandlebarsException("{{*inline}} helper must have exactly one argument");
            }

            //This helper needs the "context" var to be the complete BindingContext as opposed to just the
            //data { firstName: "todd" }. The full BindingContext is needed for registering the partial templates.
            //This magic happens in BlockHelperFunctionBinder.VisitBlockHelperExpression

            if (!(context.Value is BindingContext bindingContext))
            {
                throw new HandlebarsException("{{*inline}} helper must receiving the full BindingContext");
            }

            if(!(arguments[0] is string key)) throw new HandlebarsRuntimeException("Inline argument is not valid");
            
            //Inline partials cannot use the Handlebars.RegisterTemplate method
            //because it is static and therefore app-wide. To prevent collisions
            //this helper will add the compiled partial to a dicionary
            //that is passed around in the context without fear of collisions.
            var template = options.OriginalTemplate;
            bindingContext.InlinePartialTemplates.AddOrReplace(key, (writer, c) => template(writer, c));
        }
    }
}