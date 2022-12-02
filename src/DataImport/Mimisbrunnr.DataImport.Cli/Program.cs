using Microsoft.Extensions.Logging;
using Mimisbrunnr.DataImport.Cli;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Web.Host.Services;

static int ShowHelp(string help) { Console.WriteLine(help); return 0; }
static int OnError(string usage) { Console.WriteLine(usage); return 1; }

static int Main(CliConfiguration configuration)
{
    Console.WriteLine($@"Launch params:{{
        host        = {configuration.OptHost},
        space       = {configuration.OptSpace},
        token       = {configuration.OptToken},
        filePath    = {configuration.OptFile},
        fullFilePath    = {Path.GetFullPath(configuration.OptFile)},
        createSpace = {configuration.OptCreateSpace} 
}}");

    if (string.IsNullOrEmpty(configuration.OptFile) || !File.Exists(Path.GetFullPath(configuration.OptFile)))
        throw new ArgumentException("File path not set or file not found");

    var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug));
    var wikiService = new WikiService(configuration.OptHost, configuration.OptToken);
    var dataImportService = new ConfluenceDataImportService(wikiService, loggerFactory.CreateLogger<ConfluenceDataImportService>());

    SpaceModel space = null;
    if (configuration.OptCreateSpace)
        space = wikiService.CreateSpace(new SpaceCreateModel()
        {
            Type = SpaceTypeModel.Private,
            Key = configuration.OptSpace,
            Description = "Imported space",
            Name = $"{configuration.OptSpace} - Imported - {DateOnly.FromDateTime(DateTime.Now)}"
        }).GetAwaiter().GetResult();
    else
        space = wikiService.GetSpaceByKey(configuration.OptSpace).GetAwaiter().GetResult();

    using (var importFile = File.OpenRead(Path.GetFullPath(configuration.OptFile)))
    {
        dataImportService.ImportSpace(space, importFile).GetAwaiter().GetResult();
    }
    return 0;
}
return CliConfiguration.CreateParser()
                        .Parse(args)
                        .Match(Main,
                              result => ShowHelp(result.Help),
                              result => OnError(result.Usage));