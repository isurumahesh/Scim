using Rsk.AspNetCore.Scim.Attributes;

namespace ScimTest.Api.Extensions;

public class Site
{
    [Description("Site id.")]
    [Required(Required.Create)]
    public string Id { get; set; } = null!;

    [Description("The User's site role.  A complex type that optionally allows to set the list of roles.")]
    public IEnumerable<Role>? Roles { get; set; }
}
