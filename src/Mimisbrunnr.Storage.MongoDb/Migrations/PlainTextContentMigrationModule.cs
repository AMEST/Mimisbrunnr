using System.Text.RegularExpressions;
using DnsClient.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Modules;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Storage.MongoDb.Migrations;

public class PlainTextContentMigrationModule : BackgroundModule
{
    private static readonly Regex _markdownRemoveRegex =
    new Regex(@"__|\*|\#|(?:\[([^\]]*)\]\([^)]*\))", RegexOptions.Compiled);
    private static readonly Regex _htmlTagsRemoveRegex = new Regex(@"<.*?>", RegexOptions.Compiled);

    public override async Task ExecuteAsync(IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        var logger = provider.GetService<ILogger<PlainTextContentMigrationModule>>();
        var pageRepository = provider.GetService<IRepository<Page>>();
        var historicalPageRepository = provider.GetService<IRepository<HistoricalPage>>();
        var parallelOptions = new ParallelOptions()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Max(4, Environment.ProcessorCount)
        };
        logger.LogInformation("Start fix page plain text");
        try
        {
            var pagesWithoutPlainText = await pageRepository
                .GetAll()
                .Where(x => x.EditorType != PageEditorType.EditorJs
                    && string.IsNullOrEmpty(x.PlainTextContent)
                    && !string.IsNullOrEmpty(x.Content))
                .ToArrayAsync();
            await Parallel.ForEachAsync(pagesWithoutPlainText, parallelOptions, async (page, token) =>
            {
                page.PlainTextContent = Sanitize(page.Content);
                await pageRepository.Update(page, token);
            });
            var historicalPagesWithoutPlainText = await historicalPageRepository.GetAll()
                .Where(x => x.EditorType != PageEditorType.EditorJs
                    && string.IsNullOrEmpty(x.PlainTextContent)
                    && !string.IsNullOrEmpty(x.Content))
                .ToArrayAsync();
            await Parallel.ForEachAsync(historicalPagesWithoutPlainText, parallelOptions, async (page, token) =>
            {
                page.PlainTextContent = Sanitize(page.Content);
                await historicalPageRepository.Update(page, token);
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Start fix page plain texts");
        }
    }

    private static string Sanitize(string articleBody)
    {
        var mdSanitized = _markdownRemoveRegex.Replace(articleBody, "");
        return _htmlTagsRemoveRegex.Replace(mdSanitized, "");
    }
}