using System.Net;
using System.Net.Http.Json;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Integration.Client;

public class AttachmentService
{
    private const string ApiPath = "/api/attachment";

    private readonly HttpClient _httpClient;

    public AttachmentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AttachmentModel[]> GetAttachments(string pageId, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{pageId}", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<AttachmentModel[]>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException($"Page {pageId} not found");

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<Stream> GetAttachmentContent(string pageId, string name, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{pageId}/{name}", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadAsStreamAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException($"Attachemnt {name} not found or page {pageId} deleted");

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task Upload(string pageId, Stream content, string name, CancellationToken cancellationToken = default)
    {
        using var multipartFormContent = new MultipartFormDataContent();
        multipartFormContent.Add(new StreamContent(content), name: "attachment", fileName: name);

        var request = await _httpClient.PostAsync($"{ApiPath}/{pageId}", multipartFormContent, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }


    public async Task Remove(string pageId, string name, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.DeleteAsync($"{ApiPath}/{pageId}/{name}", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }
}