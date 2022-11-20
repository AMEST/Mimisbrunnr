using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Web.Wiki.Import;

public interface ISpaceImportService
{
    Task Import(SpaceModel spaceModel, Stream importStream, UserInfo createdBy);
}