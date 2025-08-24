using Stubble.Core;
using Stubble.Core.Settings;

namespace Mimisbrunnr.Web.Infrastructure
{
    public class StubbleTemplateRenderer : ITemplateRenderer
    {
        private readonly StubbleVisitorRenderer _stubble;

        public StubbleTemplateRenderer()
        {
            _stubble = new StubbleVisitorRenderer();
        }

        public async Task<string> Render(string template, IDictionary<string, object> parameters)
        {
            return await _stubble.RenderAsync(template, parameters, new RenderSettings
            {
                SkipHtmlEncoding = true
            });
        }
    }
}
