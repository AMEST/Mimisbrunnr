using DocoptNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mimisbrunnr.Migration.Persistent.Cli;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

static int ShowHelp(string help) { Console.WriteLine(help); return 0; }
static int OnError(IInputErrorResult result) { Console.WriteLine(result.Error); Console.WriteLine(result.Usage); return 1; }

static int Main(CliConfiguration configuration)
{
    if(string.IsNullOrEmpty(configuration.OptConnectionString))
        throw new ArgumentNullException("Connection string can't be null");
    if(string.IsNullOrEmpty(configuration.OptFromStorageType) || string.IsNullOrEmpty(configuration.OptToStorageType))
        throw new ArgumentNullException("To Storage type and From Storage type can't be null");

    var migrateFrom = configuration.GetFromStorageType();
    var migrateTo = configuration.GetToStorageType();

    if(migrateFrom == migrateTo)
        throw new InvalidOperationException("Migrate files to same storage not available");

    Console.WriteLine($"Migrate persistent from {migrateFrom} to {migrateTo}");

    var migrateFromConfiguration = configuration.GetPersistentModuleConfiguration(migrateFrom);
    var migrateToConfiguration = configuration.GetPersistentModuleConfiguration(migrateTo);

    var services = new ServiceCollection();
    services.AddLogging(x => x.AddConsole());

    services.AddFileStorage(migrateFromConfiguration);
    services.AddFileStorage(migrateToConfiguration);
    services.AddDataBase(configuration.OptConnectionString.Replace("\"",""));

    var provider = services.BuildServiceProvider();

    var attachmentRepository = provider.GetRequiredService<IRepository<Attachment>>();
    var sourceStorage = provider.GetStorage(migrateFrom);
    var targetStorage = provider.GetStorage(migrateTo);

    var attachments = attachmentRepository.GetAll().ToArray();
    var migratedCount = 0L;
    foreach(var attachment in attachments)
    {
        Console.WriteLine($"Migrating {attachment.Path}");
        if(!sourceStorage.Exist(attachment.Path).GetAwaiter().GetResult()){
            Console.WriteLine("Attachment not found");
            continue;
        }
        if(configuration.OptOnlyAbsent)
            if(targetStorage.Exist(attachment.Path).GetAwaiter().GetResult())
                continue;
                
        var currentBinary = sourceStorage.DownloadFileAsync(attachment.Path)
            .GetAwaiter().GetResult();
        targetStorage.UploadFileAsync(currentBinary.Content, attachment.Path)
            .GetAwaiter().GetResult();
        migratedCount++;
    }

    Console.WriteLine($"Migration completed. AttachmentCount: {migratedCount}");
    return 0;
}
return CliConfiguration.CreateParser()
                        .Parse(args)
                        .Match(Main,
                              result => ShowHelp(result.Help),
                              result => OnError(result));