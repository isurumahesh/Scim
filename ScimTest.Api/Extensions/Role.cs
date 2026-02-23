using Rsk.AspNetCore.Scim.Attributes;

namespace ScimTest.Api.Extensions;

public class Role
{
    [Description("User's site role.")]
    [Required(Required.Create)]
    public string Id { get; set; } = null!;
}
