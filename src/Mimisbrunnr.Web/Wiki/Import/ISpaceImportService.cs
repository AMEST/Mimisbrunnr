using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Wiki.Import;

public interface ISpaceImportService
{
    Task Import(SpaceModel spaceModel, Stream importStream, UserInfo createdBy);
}