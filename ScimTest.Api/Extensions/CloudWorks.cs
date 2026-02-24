using Rsk.AspNetCore.Scim.Attributes;
using Rsk.AspNetCore.Scim.Models;

namespace ScimTest.Api.Extensions;

public class CloudWorks : ResourceExtension
{
    public const string Schema = "urn:ietf:params:scim:schemas:extension:cloudworks:1.0:User";

    [Description(
        "Sites user belongs to. A complex type that optionally allows to set the list of sites user is part of."
    )]
    public string SiteId { get; set; }
}


public class Enterprise : ResourceExtension
{
    public const string Schema = "urn:ietf:params:scim:schemas:extension:enterprise:2.0:User";
    
    public string Department { get; set; }
}
