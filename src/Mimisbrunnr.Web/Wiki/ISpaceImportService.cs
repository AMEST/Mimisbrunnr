using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Wiki
{
    public interface ISpaceImportService
    {
         Task Import(SpaceModel spaceModel, Stream importStream, UserInfo createdBy);
    }
}