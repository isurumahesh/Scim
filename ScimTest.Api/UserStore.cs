
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;
namespace ScimTest.Api;

public class UserStore : IScimStore<User>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserWriteRepository _userWriteRepository;

    public UserStore(IUserReadOnlyRepository userReadOnlyRepository, IUserWriteRepository userWriteRepository)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _userWriteRepository = userWriteRepository;
    }

    public Task<IEnumerable<string>> Exists(IEnumerable<string> ids) => _userReadOnlyRepository.Exists(ids);

    public Task<User> Add(User resource) => _userWriteRepository.Add(resource);

    public Task<User> GetById(string id, ResourceAttributeSet attributes) =>
        _userReadOnlyRepository.GetById(id, attributes);

    public Task<ScimPageResults<User>> GetAll(IIndexResourceQuery query) => _userReadOnlyRepository.GetAll(query);

    public Task<ScimCursorPageResults<User>> GetAll(ICursorResourceQuery query) =>
        _userReadOnlyRepository.GetAll(query);

    public Task<User> Update(User resource) => _userWriteRepository.Update(resource);

    public Task<User?> PartialUpdate(string resourceId, IEnumerable<PatchCommand> commands) =>
        _userWriteRepository.PartialUpdate(resourceId,commands);

    public Task Delete(string id) => _userWriteRepository.Delete(id);
}
