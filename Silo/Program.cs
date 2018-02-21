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
using OrleansDashboard;

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
            var config = new ClusterConfiguration();

            config.Defaults.Port = 11111;  
            config.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Any, 30000);

            config.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.Disabled;
            config.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.Custom;               
            config.Globals.MembershipTableAssembly = "Orleans.Clustering.DynamoDB";

            config.Globals.ClusterId = "Cluster";

            config.AddMemoryStorageProvider();
            //config.RegisterDashboard();

            var host = new SiloHostBuilder()
                .UseConfiguration(config)
                /*.UseDashboard(options => {
                    options.HostSelf = true;
                    options.HideTrace = true;
                })*/
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole())
                .ConfigureHostConfiguration(configBuilder => {
                    configBuilder.AddInMemoryCollection(new [] {
                        new KeyValuePair<string, string>(HostDefaults.EnvironmentKey, EnvironmentName.Development),
                        new KeyValuePair<string, string>(HostDefaults.ApplicationKey, "OrleansSimpleTestApp")
                    });
                })
                .UseDynamoDBMembership(options => {
                    options.ConnectionString = ConnectionString;
                })
                .Build();

            await host.StartAsync();

            return host;
        }
    }
}