using System.Web;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Core.Interfaces;

namespace Mimisbrunnr.Web.Infrastructure
{
    public class StubbleTemplateRenderer : ITemplateRenderer
    {
        private readonly IAsyncStubbleRenderer _stubble;

        public StubbleTemplateRenderer()
        {
            _stubble = new StubbleBuilder()
                .Configure(settings =>
                {
                    settings.SetIgnoreCaseOnKeyLookup(true);
                })
                .Build();
        }

        public async Task<string> Render(string template, IDictionary<string, object> parameters)
        {
            parameters.Add(nameof(UrlEncode), UrlEncode);
            return await _stubble.RenderAsync(template, parameters);
        }

        private static object UrlEncode(string str, Func<string, string> render)
        {
            return HttpUtility.UrlEncode(render(str));
        }
    }
}
