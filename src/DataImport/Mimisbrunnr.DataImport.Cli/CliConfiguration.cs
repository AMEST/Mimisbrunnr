using DocoptNet;

namespace Mimisbrunnr.DataImport.Cli;

[DocoptArguments]
public partial class CliConfiguration
{
    const string Help = @"Mimisbrunnr Wiki data import cli.

    Usage:
      mimisbrunnr-import-cli.exe --host=<wiki url> --space=<space key> --token=<access token> --file=<path to exported file> [--create-space]
      mimisbrunnr-import-cli.exe (-h | --help)

    Options:
      --host=<wiki url>             Base url to Mimisbrunnr Wiki instance
      --space=<key>                 Space key
      --token=<jwt>                 Access token for Mimisbrunnr Wiki
      --file=<path>                 File path to exported file
      --create-space                Need create space when import
      -h --help                     Show this screen.
";
}