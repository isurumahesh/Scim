using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;

namespace ScimTest.Api;

public interface IUserReadOnlyRepository
{
    Task<ScimPageResults<User>> GetAll(IIndexResourceQuery query);

    Task<ScimCursorPageResults<User>> GetAll(ICursorResourceQuery query);

    Task<User> GetById(string id, ResourceAttributeSet attributes);

    Task<IEnumerable<string>> Exists(IEnumerable<string> ids);
}
