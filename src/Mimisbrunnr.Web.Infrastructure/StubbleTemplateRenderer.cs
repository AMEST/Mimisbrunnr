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
            parameters.Add(nameof(Uuid), Uuid);
            return await _stubble.RenderAsync(template, parameters);
        }

        private static object Uuid(string _, Func<string, string> __)
        {
            return Guid.NewGuid().ToString("N");
        }

        private static object UrlEncode(string str, Func<string, string> render)
        {
            return HttpUtility.UrlEncode(render(str));
        }

        private static object HtmlEncode(string str, Func<string, string> render)
        {
            return HttpUtility.HtmlEncode(render(str));
        }

        private static object ToLower(string str, Func<string, string> render)
        {
            return render(str).ToLowerInvariant();
        }

        private static object ToUpper(string str, Func<string, string> render)
        {
            return render(str).ToUpperInvariant();
        }
    }
}
