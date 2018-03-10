using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using GrainCollection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Orleans.Configuration;
using OrleansAWSUtils;

namespace OrleansSiloHost
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        public static string ConnectionString = "<ConnectionStrHere>";
        private static async Task<ISiloHost> StartSilo()
        {
            var host = new SiloHostBuilder()
                .Configure(options => {
                    options.ClusterId = "TestCluster";
                })
                .ConfigureEndpoints(IPAddress.Any, 30000, 11111)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole())
                .ConfigureHostConfiguration(configBuilder => {
                    configBuilder.AddInMemoryCollection(new [] {
                        new KeyValuePair<string, string>(HostDefaults.EnvironmentKey, EnvironmentName.Development),
                        new KeyValuePair<string, string>(HostDefaults.ApplicationKey, "OrleansSimpleTestApp")
                    });
                })
                .UseDynamoDBClustering(options => {
                    options.Service = "";
                    options.TableName = "OrleansSiloInstances";
                })
                .Build();

            await host.StartAsync();

            return host;
        }
    }
}