using System.Collections.Generic;
using Xunit;

namespace HandlebarsDotNet.Test
{
    public class ReadmeTests
    {
        [Fact]
        public void RegisterBlockHelper()
        {
            var handlebars = Handlebars.Create();
            handlebars.RegisterHelper("StringEqualityBlockHelper", (output, options, context, arguments) => 
            {
                if (arguments.Length != 2)
                {
                    throw new HandlebarsException("{{#StringEqualityBlockHelper}} helper must have exactly two arguments");
                }

                var left = arguments.At<string>(0);
                var right = arguments[1] as string;
                if (left == right) options.Template(output, context);
                else options.Inverse(output, context);
            });

            var animals = new Dictionary<string, string> 
            {
                {"Fluffy", "cat" },
                {"Fido", "dog" },
                {"Chewy", "hamster" }
            };

            var template = "{{#each this}}The animal, {{@key}}, {{#StringEqualityBlockHelper @value 'dog'}}is a dog{{else}}is not a dog{{/StringEqualityBlockHelper}}.\r\n{{/each}}";
            var compiledTemplate = handlebars.Compile(template);
            string templateOutput = compiledTemplate(animals);
            
            Assert.Equal(
                "The animal, Fluffy, is not a dog.\r\n" + 
                         "The animal, Fido, is a dog.\r\n" + 
                         "The animal, Chewy, is not a dog.\r\n", 
                templateOutput
            );
        }
        
        [Fact]
        public void RegisterHelper()
        {
            var source = @"Click here: {{link_to}}";
            
            var handlebars = Handlebars.Create();
            handlebars.RegisterHelper("link_to", (writer, context, parameters) =>
            {
                writer.WriteSafeString($"<a href='{context["url"]}'>{context["text"]}</a>");
            });
            
            var template = handlebars.Compile(source);

            var data = new {
                url = "https://github.com/rexm/handlebars.net",
                text = "Handlebars.Net"
            };

            var result = template(data);
            
            Assert.Equal($"Click here: <a href='{data.url}'>{data.text}</a>", result);
        }
    }
}