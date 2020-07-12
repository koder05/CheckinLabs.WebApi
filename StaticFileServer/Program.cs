using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System;

namespace StaticFileServer
{
    class Program
    {
        static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
            .Configure(config => config.UseStaticFiles())
            .UseWebRoot("wwwroot")
            .UseUrls("http://+:5004")
            .Build().Run();
        }
    }
}
