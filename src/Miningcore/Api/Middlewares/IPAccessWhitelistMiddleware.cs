using System.Net;
using Microsoft.AspNetCore.Http;
using Miningcore.Extensions;
using NLog;

namespace Miningcore.Api.Middlewares;

public class IPAccessWhitelistMiddleware
{
    private readonly bool gpdrCompliantLogging;
    private readonly string[] locations;
    private readonly ILogger logger = LogManager.GetCurrentClassLogger();

    private readonly RequestDelegate next;
    private readonly IPAddress[] whitelist;

    public IPAccessWhitelistMiddleware(RequestDelegate next, string[] locations, IPAddress[] whitelist, bool gpdrCompliantLogging)
    {
        this.whitelist = whitelist;
        this.next = next;
        this.locations = locations;
        this.gpdrCompliantLogging = gpdrCompliantLogging;
    }

    public async Task Invoke(HttpContext context)
    {
        if(locations.Any(x => context.Request.Path.Value.StartsWith(x)))
        {
            var remoteAddress = context.Connection.RemoteIpAddress;
            if(!whitelist.Any(x => x.Equals(remoteAddress)))
            {
                logger.Info(() => $"Unauthorized request attempt to {context.Request.Path.Value} from {remoteAddress.CensorOrReturn(gpdrCompliantLogging)}");

                context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("You are not in my access list. Good Bye.\n");
                return;
            }
        }

        await next.Invoke(context);
    }
}
