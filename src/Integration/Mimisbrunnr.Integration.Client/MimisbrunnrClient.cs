namespace Mimisbrunnr.Integration.Client;

public sealed class MimisbrunnrClient
{
    private readonly HttpClient _httpClient;

    public MimisbrunnrClient(HttpClient httpClient, MimisbrunnrClientConfiguration configuration)
    {
        _httpClient = httpClient;
        httpClient.BaseAddress = new Uri(configuration.Host);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration.AccessToken}");
        
        Attachments = new AttachmentService(_httpClient);
        Pages = new PageService(_httpClient);
        Spaces = new SpaceService(_httpClient);
        Users = new UserService(_httpClient);
        Groups = new GroupService(_httpClient);
    }

    public AttachmentService Attachments { get; }

    public PageService Pages { get; }

    public SpaceService Spaces { get; }

    public UserService Users { get; }

    public GroupService Groups { get; }

}