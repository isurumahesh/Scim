using Microsoft.EntityFrameworkCore;
using Rsk.AspNetCore.Scim.Constants;
using Rsk.AspNetCore.Scim.Exceptions;
using Rsk.AspNetCore.Scim.Filters;
using Rsk.AspNetCore.Scim.Models;
using Rsk.AspNetCore.Scim.Stores;
using System.Linq.Expressions;
using System.Linq;


namespace ScimTest.Api;

public class UserReadOnlyRepository : IUserReadOnlyRepository
{
    private readonly IScimQueryBuilderFactory queryBuilderFactory;
    private readonly ILogger<UserReadOnlyRepository> _logger;
    private readonly AppDbContext ctx;
    private readonly IPatchCommandExecutor patchCommandExecutor;


    public UserReadOnlyRepository(
        IScimQueryBuilderFactory queryBuilderFactory,
        ILogger<UserReadOnlyRepository> logger,
        AppDbContext ctx,
        IPatchCommandExecutor patchCommandExecutor
      
    )
    {
        this.queryBuilderFactory = queryBuilderFactory;
        _logger = logger;
        this.ctx = ctx;
        this.patchCommandExecutor = patchCommandExecutor;
    }

    public async Task<IEnumerable<string>> Exists(IEnumerable<string> ids)
    {
        return await ctx.Users.Select(u => u.Id).Intersect(ids).ToListAsync();
    }

    public async Task<User> GetById(string id, ResourceAttributeSet attributes)
    {
        var user = await FindUser(id);

        return MapAppUserToScimUser(user);
    }

    public async Task<ScimPageResults<User>> GetAll(IIndexResourceQuery query)
    {
        if (query.Filter.IsExternalIdEqualityExpression(out string? id))
        {
            return new ScimPageResults<User>(Enumerable.Empty<User>(), 0);
        }

        IQueryable<AppUser> baseQuery = ctx.Users;

        IQueryable<AppUser> databaseQuery =
            queryBuilderFactory.CreateQueryBuilder<AppUser>(baseQuery)
                .Filter(query.Filter)
                .Build();

        IQueryable<AppUser> pageQuery = queryBuilderFactory.CreateQueryBuilder<AppUser>(databaseQuery)
            .Page(query.StartIndex, query.Count)
            .Sort(query.Sort.By, query.Sort.Direction)
            .Build();

        int totalCount = await databaseQuery.CountAsync();

        var matchingUsers = pageQuery
            .ToList()
            .Select(MapAppUserToScimUser);

        return new ScimPageResults<User>(matchingUsers, totalCount);
    }

    public async Task<ScimCursorPageResults<User>> GetAll(ICursorResourceQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Cursor) && !Guid.TryParse(query.Cursor, out _))
        {
            throw new ScimStoreUnrecognizedCursorException("Cursor is not a valid GUID");
        }

        IQueryable<AppUser> sortedSet = ctx.Users
            .OrderBy(u => u.Id);

        IQueryable<AppUser> databaseQuery =
            queryBuilderFactory.CreateQueryBuilder(sortedSet)
                .Filter(query.Filter)
                .Build();

        IQueryable<AppUser> skipQuery = databaseQuery;

        if (!string.IsNullOrWhiteSpace(query.Cursor))
        {
            skipQuery = skipQuery.Where(q => string.Compare(q.Id, query.Cursor) > 0);
        }

        int totalCount = await databaseQuery.CountAsync();

        string? nextCursor = query.Count == int.MaxValue ? null :
            await skipQuery
                .Skip(query.Count + 1)
                .Take(1)
                .Select(sq => sq.Id)
                .FirstOrDefaultAsync();

        IQueryable<AppUser> pageQuery = queryBuilderFactory.CreateQueryBuilder(skipQuery)
            .Sort(query.Sort.By, query.Sort.Direction)
            .Build();

        var matchingUsers = pageQuery
            .ToList()
            .Select(MapAppUserToScimUser);

        string? previousCursor = string.IsNullOrWhiteSpace(query.Cursor) ? null : query.Cursor;

        return new ScimCursorPageResults<User>(matchingUsers, totalCount, nextCursor, previousCursor);
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
            },
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
}
