using Microsoft.EntityFrameworkCore;
using Rsk.AspNetCore.Scim.Exceptions;
using Rsk.AspNetCore.Scim.Filters;
using Rsk.AspNetCore.Scim.Hosting.Tenancy;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;
using ScimTest.Api.Extensions;

namespace ScimTest.Api;

public class UserWriteRepository : IUserWriteRepository
{
    private readonly ILogger<UserWriteRepository> logger;
    private readonly AppDbContext ctx;
    private readonly IPatchCommandExecutor patchCommandExecutor;

    public UserWriteRepository(
        ILogger<UserWriteRepository> logger,
        AppDbContext ctx,
        IPatchCommandExecutor patchCommandExecutor
    )
    {
     
        this.logger = logger;
        this.ctx = ctx;
        this.patchCommandExecutor = patchCommandExecutor;
    }

    public async Task<User> Add(User resource)
    {
        IEnumerable<Site> sites = GetCloudWorksSites(resource);
        var user = MapScimUserToAppUser(resource, new AppUser());
        user.LastName = sites.First().Id;
        
        await ctx.Users.AddAsync(user);
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw new ScimStoreItemAlreadyExistException($"User with userName: {resource.UserName} already exists");
        }

        var x= MapAppUserToScimUser(user);
        resource.Id = x.Id;
        return resource;
    }

    public async Task<User> Update(User resource)
    {
        AppUser user = await FindUser(resource.Id);

        MapScimUserToAppUser(resource, user);

        await ctx.SaveChangesAsync();

        return MapAppUserToScimUser(user);
        
    }

    public async Task<User?> PartialUpdate(string resourceId, IEnumerable<PatchCommand> commands)
    {
        AppUser user = await FindUser(resourceId);

        foreach (PatchCommand replaceCmd in commands)
        {
            try
            {
                patchCommandExecutor.Execute(user, replaceCmd);
            }
            catch (ScimStorePatchException)
            {
                logger.LogError("Failed to patch user with id {id} with command {cmd}", resourceId, replaceCmd);
            }
        }

        await ctx.SaveChangesAsync();

        return null;
    }

    public async Task Delete(string id)
    {
        AppUser user = await FindUser(id);

        user.IsDisabled = true;

        await ctx.SaveChangesAsync();
    }
    
    private async Task<AppUser> FindUser(string id)
    {
        AppUser? user = await ctx.Users
            .SingleOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new ScimStoreItemDoesNotExistException($"User with id '{id}' not found");
        }

        return user;
    }
    
    private static User MapAppUserToScimUser(AppUser user)
    {
        return new User
        {
            Id = user.Id,
            UserName = user.Username,
            Active = !user.IsDisabled,
            DisplayName = user.DisplayName,
            NickName = user.Nickname,
            ProfileUrl = user.ProfileUrl,
            Title = user.Title,
            Timezone = user.Timezone,
            PreferredLanguage = user.PreferredLanguage,
            UserType = user.UserType,
            Locale = user.Locale,
            Name = new Name()
            {
                Formatted = user.Formatted,
                FamilyName = user.LastName,
                GivenName = user.FirstName,
                MiddleName = user.MiddleName,
                HonorificSuffix = user.HonorificSuffix,
                HonorificPrefix = user.HonorificPrefix
            }
        };
    }
    
     private static AppUser MapScimUserToAppUser(User resource, AppUser user)
    {
        string? primaryEmail = resource
            .Emails?
            .SingleOrDefault(e => e.Primary == true)
            ?.Value;

        if (primaryEmail == null)
        {
            primaryEmail = resource.UserName;
        }
        
        
        user.Username = resource.UserName ?? primaryEmail;
        user.Locale = resource.Locale ?? user.Locale;
        user.FirstName = resource.Name?.GivenName ?? user.FirstName;
        user.LastName = resource.Name?.FamilyName ?? user.LastName;
        user.NormalizedUsername = user.Username?.ToUpper();
        user.Nickname = resource.NickName ?? user.Nickname;
        user.Title = resource.Title ?? user.Title;
        user.DisplayName = resource.DisplayName ?? user.DisplayName;
        user.Timezone = resource.Timezone ?? user.Timezone;
        user.ProfileUrl = resource.ProfileUrl ?? user.ProfileUrl;
        user.Formatted = resource.Name?.Formatted ?? user.Formatted;
        user.MiddleName = resource.Name?.MiddleName ?? user.MiddleName;
        user.HonorificSuffix = resource.Name?.HonorificSuffix ?? user.HonorificSuffix;
        user.HonorificPrefix = resource.Name?.HonorificPrefix ?? user.HonorificPrefix;
        user.IsDisabled = !resource.Active ?? user.IsDisabled;
        user.PreferredLanguage = resource.PreferredLanguage ?? user.PreferredLanguage;
        user.UserType = resource.UserType;
        
        return user;
    }
     
    private IEnumerable<Site> GetCloudWorksSites(User resource)
    {
        if (resource.Extensions is null)
        {
            return [];
        }

        if (!resource.Extensions.TryGetValue(CloudWorks.Schema, out ResourceExtension? ext))
        {
            return [];
        }

        if (ext is not CloudWorks cloudWorks)
        {
            return [];
        }

        return cloudWorks.Sites ?? [];
    }
}