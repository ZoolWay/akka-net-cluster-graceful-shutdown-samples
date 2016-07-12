using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace ClusterWebNode2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            global::log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();
            host.Run();
        }
    }
}
