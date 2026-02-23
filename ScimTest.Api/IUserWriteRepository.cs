using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;

namespace ScimTest.Api;

public interface IUserWriteRepository
{
    Task<User> Add(User resource);

    Task<User> Update(User resource);

    Task<User?> PartialUpdate(string resourceId, IEnumerable<PatchCommand> commands);

    Task Delete(string id);
}
