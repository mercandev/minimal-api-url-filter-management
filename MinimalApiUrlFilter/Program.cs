using Microsoft.Extensions.Caching.Redis;
using MinimalApiUrlFilter.Cache;
using MinimalApiUrlFilter.Const;
using MinimalApiUrlFilter.Domain;
using MinimalApiUrlFilter.Service;
using System.Net;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("ConnectionString");
builder.Services.AddDbContext<UrlFilterContext>(x => x.UseNpgsql(connectionString));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

RedisCacheOptions options = new();
var section = builder.Configuration.GetSection("Redis");
section.Bind(options);

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
builder.Services.AddSingleton<IUrlFilterService, UrlFilterService>();
builder.Services.AddSingleton<RedisCacheOptions>(options);

builder.Services.AddDistributedRedisCache(option =>
{
    option.Configuration = options.Configuration;
    option.InstanceName = options.InstanceName;
});


var app = builder.Build();
app.UseSwagger();

app.MapGet("/UrlCheck", (string queryUrlFilter, IUrlFilterService urlFilterService, HttpContext http) =>
{
    if (string.IsNullOrWhiteSpace(queryUrlFilter))
    {
        throw new Exception(ErrorConst.URL_CANNOT_BE_EMPTY);
    }

    if (!urlFilterService.IsUrlValid(queryUrlFilter))
    {
        throw new Exception(ErrorConst.URL_ADDRESS_NOT_VALID);
    }

    var key = urlFilterService.UrlKey(queryUrlFilter);

    if (string.IsNullOrWhiteSpace(key.Item1))
    {
        throw new Exception(ErrorConst.REDIS_KEY_ERROR);
    }

    var domainCheck = urlFilterService.GetUrlRedis<UrlFilterContentModel>(key.Item1);

    if (domainCheck is not null && domainCheck.DomainBlocked)
    {
        return http.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
    }

    var result = urlFilterService.GetUrlRedis<UrlFilterContentModel>(key.Item2);

    return result == null ? http.Response.StatusCode = (int)HttpStatusCode.Accepted : http.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
});

app.MapPost("/AddNewUrl", (UrlFilterContext context, UrlFilterContentModel? contentModel, IUrlFilterService urlFilterService, HttpContext http) =>
{
    if (contentModel is null)
    {
        throw new Exception(ErrorConst.CONTENT_MODEL_NOT_VALID);
    }

    if (string.IsNullOrWhiteSpace(contentModel.Url))
    {
        throw new Exception(ErrorConst.URL_CANNOT_BE_EMPTY);
    }

    var key = urlFilterService.UrlKey(contentModel.Url);

    UrlFilterContentModel urlFilterContent = new()
    {
        AllPortsBlocked = contentModel.AllPortsBlocked,
        NonSecureAccess = contentModel.NonSecureAccess,
        SecureAccess = contentModel.SecureAccess,
        DomainBlocked = contentModel.DomainBlocked ,
        Url = contentModel.Url
    };

    var keyHash = urlFilterContent.DomainBlocked.Equals(true) ? key.Item1 : key.Item2;

    urlFilterService.SetUrlRedis(keyHash, urlFilterContent, TimeSpan.FromDays(1));


    var dublicateControl = context.UrlFilter.Where(x => x.UrlKey == keyHash);

    if (dublicateControl.Any())
    {
        throw new Exception("url already exist!");
    }
    
    var addDbFilter = new UrlFilter()
    {
        UrlKey = keyHash,
        UrlContent = contentModel.Url,
        NonSecureAccess = contentModel.NonSecureAccess,
        SecureAccess = contentModel.SecureAccess,
        AllPortsBlocked = contentModel.AllPortsBlocked,
        DomainBlocked = contentModel.DomainBlocked,
        CreatedBy = "App"
    };

    context.UrlFilter.Add(addDbFilter);
    context.SaveChanges();

    return http.Response.StatusCode = (int)HttpStatusCode.Created;
});

app.MapDelete("/DeleteUrl", (UrlFilterContext context, string queryUrlFilter, IUrlFilterService urlFilterService, HttpContext http) =>
{
    if (string.IsNullOrWhiteSpace(queryUrlFilter))
    {
        throw new Exception(ErrorConst.URL_CANNOT_BE_EMPTY);
    }

    if (!urlFilterService.IsUrlValid(queryUrlFilter))
    {
        throw new Exception(ErrorConst.URL_ADDRESS_NOT_VALID);
    }

    var key = urlFilterService.UrlKey(queryUrlFilter);

    var deleteKey = (string.IsNullOrWhiteSpace(key.Item2) ? key.Item1 : key.Item2);

    var deleteUrl = context.UrlFilter.Where(x => x.UrlKey == deleteKey).FirstOrDefault();

    if (deleteUrl is null) 
    {
        throw new Exception("url not exist!");
    }

    deleteUrl.IsActive = false;
    urlFilterService.RemoveUrlRedis(deleteKey);
    
    context.UrlFilter.Update(deleteUrl);
    context.SaveChanges();
    
    return http.Response.StatusCode = (int)HttpStatusCode.OK;
});


app.MapPut("/UpdateUrl", (UrlFilterContext context, UrlFilterContentModel? contentModel, IUrlFilterService urlFilterService, HttpContext http) =>
{

    if (contentModel is null)
    {
        throw new Exception(ErrorConst.CONTENT_MODEL_NOT_VALID);
    }
    
    if (!urlFilterService.IsUrlValid(contentModel.Url))
    {
        throw new Exception(ErrorConst.URL_ADDRESS_NOT_VALID);
    }

    var checkUrl = context.UrlFilter.Where(x => x.UrlContent == contentModel.Url).FirstOrDefault();

    if (checkUrl is null)
    {
        throw new Exception("url not exist!");
    }
    
    var key = urlFilterService.UrlKey(contentModel.Url);
    
    var keyHash = contentModel.DomainBlocked.Equals(true) ? key.Item1 : key.Item2;

    var updateModel = new UrlFilterContentModel()
    {
        Url = contentModel.Url,
        DomainBlocked = contentModel.DomainBlocked,
        SecureAccess = contentModel.SecureAccess,
        AllPortsBlocked = contentModel.AllPortsBlocked,
        NonSecureAccess = contentModel.NonSecureAccess
    };
    
    urlFilterService.SetUrlRedis(keyHash, updateModel, TimeSpan.FromDays(1)); //bug

    checkUrl.UrlContent = contentModel.Url;
    checkUrl.DomainBlocked = contentModel.DomainBlocked;
    checkUrl.SecureAccess = contentModel.SecureAccess;
    checkUrl.AllPortsBlocked = contentModel.AllPortsBlocked;
    checkUrl.NonSecureAccess = contentModel.NonSecureAccess;
    checkUrl.UpdatedDate = DateTime.Now;
    checkUrl.UpdatedBy = "App";
    
    context.UrlFilter.Update(checkUrl);
    context.SaveChanges();
    
    return http.Response.StatusCode = (int)HttpStatusCode.OK; //next feature
});


app.MapGet("/ListAllBlockedUrls", (UrlFilterContext context, IUrlFilterService urlFilterService, HttpContext http) =>
{
    return context.UrlFilter.Where(x => x.IsActive.Equals(true)).ToList();
});


app.UseSwaggerUI();

app.Run();
