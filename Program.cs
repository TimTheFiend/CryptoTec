using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoTec
{
    public class Program
    {
        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                #region `ConfigureServices has been added.`
                .ConfigureServices((context, services) => {
                    HostConfig.CertPath = context.Configuration["CertPath"];
                    HostConfig.CertPassword = context.Configuration["CertPassword"];
                })
            #endregion
                .ConfigureWebHostDefaults(webBuilder => {
                    #region This has been changed.
                    var host = System.Net.Dns.GetHostEntry("cryptotec.dk");
                    webBuilder.ConfigureKestrel(opt => {
                        opt.Listen(host.AddressList[0], 80);
                        opt.Listen(host.AddressList[0], 443, listOpt => {
                            listOpt.UseHttps(HostConfig.CertPath, HostConfig.CertPassword);
                        });
                    });
                    #endregion
                    webBuilder.UseStartup<Startup>();
                });
    }

    public static class HostConfig
    {
        public static string CertPath { get; set; }
        public static string CertPassword { get; set; }
    }

}
