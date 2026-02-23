using System.Text.Json;
using Rsk.AspNetCore.Scim.Enums;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Results;
using Rsk.AspNetCore.Scim.Validators;

public sealed class ScimUserValidator : IScimValidator<User>
{
    public Task<IScimResult<User>> ValidateUpdate(string resourceId, string resourceAsString, string schema) =>
        throw new NotImplementedException();

    public Task<IScimResult<User>> ValidateAdd(string resourceAsString, string schema)
    {
        return Validate(resourceAsString, schema, isUpdate: false);
    }
    
    private Task<IScimResult<User>> Validate(string resourceAsString, string schema, bool isUpdate)
    {
        var doc = JsonDocument.Parse(resourceAsString).RootElement;
        
       // TODO:Validation logic
       
        var user = JsonSerializer.Deserialize<User>(
            resourceAsString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return Task.FromResult<IScimResult<User>>(ScimResult<User>.Success(user!));
    }
    
}
