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
            parameters.Add(nameof(HtmlEncode), HtmlEncode);
            parameters.Add(nameof(ToLower), ToLower);
            parameters.Add(nameof(ToUpper), ToUpper);
            return await _stubble.RenderAsync(template, parameters);
        }

        private static string UrlEncode(string str, Func<string, string> render)
        {
            return HttpUtility.UrlEncode(render(str));
        }

        private static string HtmlEncode(string str, Func<string, string> render)
        {
            return HttpUtility.HtmlEncode(render(str));
        }

        private static string ToLower(string str, Func<string, string> render)
        {
            return render(str).ToLowerInvariant();
        }

        private static string ToUpper(string str, Func<string, string> render)
        {
            return render(str).ToUpperInvariant();
        }
    }
}
