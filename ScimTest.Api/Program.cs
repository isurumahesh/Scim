using Microsoft.EntityFrameworkCore;
using Rsk.AspNetCore.Scim.Configuration;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Models;
using ScimTest.Api;
using ScimTest.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options => { options.UseSqlite("Data Source=app.db"); });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IUserReadOnlyRepository, UserReadOnlyRepository>();
builder.Services.AddTransient<IUserWriteRepository, UserWriteRepository>();

builder.Services.AddScimServiceProvider("/SCIM",
        new ScimLicensingOptions()
        {
            Licensee = "Demo",
            LicenseKey =
                "eyJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjYtMDMtMTBUMDA6MDA6MDguNzMzNTYzMSswMDowMCIsImlhdCI6IjIwMjYtMDItMDhUMDA6MDA6MDkuMTg4NTU5OFoiLCJvcmciOiJERU1PIiwiYXVkIjo4fQ==.fw9HXAGtulXyZu+l+G32mhINv9732b2HacHBCTNg/TJ8n6asHM8Ll3nvzekqY/uK+CZSSRx2wxeUKxreiH15IcDzgGBNe8g0d0BcJ9sMj6mNj7SrbsneBKmTs/temFDTQBS3d06kx7DzJRhxa1dx4rwgRdRz962geet8raUB/oX/1Wvd3EqLrFYgTFi213VhmZ/lVMJVH6hdfuUtrEF7gboxW5vv9lNY/1qEMLbTxWDBoPXEECF911xhgTzdSHohRTAJBAcBOAZCSkDzsViGsIejGCS88xD7zcXfXGdDh2Lh8u+9f3uIaVzBHAkCzjNp4cgr35WQj4y2qRo76ZADdjOt3DtiTKms0HCqIUcY9f8AuWiWQQfeXuokEy/AOZ4rTgDY5cRHBp2U/IBR/+zmVB7tq1L4tPtkjTgOdWErEBTKHf/BmU92lQwNZ8lbUE6FmAEHCwArk8eCLGeQcQqS8cNpxWcYCaL1jBtPMJnzF12KjZNsF54f8jEHyRFNaT1fDilHK00wbIUu0Ix+4g9J4oXT4seDkvJ46LJ3rBpFQqx2wsDNXpYEfW+vzkRHHJJJcrMle6IABVFYBZ1Q90FaVZc0f/2NscG0+BviMXPnK7gcy+KBF96CdpjHcCxv/39v9b5Lzg6lxjkdW6mYbxsj7xGlepUwjsEarfmDRaUP5UA="
        },
        new ScimServiceProviderConfigOptions()
        {
            FilteringSupported = false,
            SortingSupported = true,
            PatchSupported = true,
            EnableAzureAdCompatibility = true,
            PaginationOptions = new PaginationOptions(true, true, PaginationMethod.Index)
        })
    .AddResource<User, UserStore>(ScimSchemas.User, "users")
    .AddResourceExtension<User, CloudWorks>(CloudWorks.Schema)
  //  .AddResourceExtension<User, Enterprise>(Enterprise.Schema)
   // .AddResource<Group, AppRoleStore>(ScimSchemas.Group, "groups")
    .AddFilterPropertyExpressionCompiler()
    .MapScimAttributes<AppUser>(ScimSchemas.User, mapper =>
    {
        mapper
            .Map("id", u => u.Id)
            .Map("userName", u => u.Username)
            .Map("active", u => u.IsDisabled, ScimFilterAttributeConverters.Inverse)
            .Map("displayName", u => u.DisplayName)
            .Map("title", u => u.Title)
            .Map("preferredLanguage", u => u.PreferredLanguage)
            .Map("userType", u => u.UserType)
            .Map("nickName", u => u.Nickname)
            .Map("userName", u => u.Username)
            .Map("timezone", u => u.Timezone)
            .Map("profileUrl", propertyAccessor: u => u.ProfileUrl)
            .Map("locale", u => u.Locale)
            .Map("name.givenName", u => u.FirstName)
            .Map("name.familyName", u => u.LastName)
            .Map("name.formatted", u => u.Formatted)
            .Map("name.middleName", propertyAccessor: u => u.MiddleName)
            .Map("name.honorificPrefix", propertyAccessor: u => u.HonorificPrefix)
            .Map("name.honorificSuffix", u => u.HonorificSuffix);
    });

var app = builder.Build();
app.UseScim();


app.UseSwagger();
app.UseSwaggerUI();


using var scope = app.Services.CreateScope();
AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
DatabaseInitializer.Initialize(context);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



app.Run();

public partial class Program
{
}