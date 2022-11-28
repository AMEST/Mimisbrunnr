using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.DataImport;

public interface IDataImportService
{
    Task ImportSpace(SpaceModel spaceModel, Stream importStream);
}