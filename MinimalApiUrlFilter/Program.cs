using Microsoft.Extensions.Caching.Redis;
using MinimalApiUrlFilter.Cache;
using MinimalApiUrlFilter.Const;
using MinimalApiUrlFilter.Domain;
using MinimalApiUrlFilter.Register;
using MinimalApiUrlFilter.Service;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapPost("/AddNewUrl", (UrlFilterContentModel contentModel, IUrlFilterService urlFilterService, HttpContext http) =>
{
    if (contentModel is null || contentModel == default)
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

    return http.Response.StatusCode = (int)HttpStatusCode.Created;
});

app.MapDelete("/DeleteUrl", (string queryUrlFilter, IUrlFilterService urlFilterService, HttpContext http) =>
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

    var isUrlSet = urlFilterService.IsSet(deleteKey);

    if (!isUrlSet)
    {
        return http.Response.StatusCode = (int)HttpStatusCode.NotFound; 
    }

    urlFilterService.RemoveUrlRedis(deleteKey);

    return http.Response.StatusCode = (int)HttpStatusCode.OK;
});


app.MapPut("/UpdateUrl", (UrlFilterContentModel contentModel, IUrlFilterService urlFilterService, HttpContext http) =>
{
    return http.Response.StatusCode = (int)HttpStatusCode.OK; //next feature
});


app.MapGet("/ListAllBlockedUrls", (IUrlFilterService urlFilterService, HttpContext http) =>
{
    return http.Response.StatusCode = (int)HttpStatusCode.OK; //next feature
});


app.UseSwaggerUI();

app.Run();
