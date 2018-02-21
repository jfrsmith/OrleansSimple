using GrainInterfaces;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using OrleansAWSUtils.Membership;

namespace OrleansClient
{
    /// <summary>
    /// Orleans test silo client
    /// </summary>
    public class Program
    {
        static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = await StartClientWithRetries())
                {
                    await DoClientWork(client);
                    Console.ReadKey();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }

        public static string ConnectionString = "<ConnectionStrHere>";
        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
        {
            int attempt = 0;
            IClusterClient client;
            while (true)
            {
                try
                {
                    var config = ClientConfiguration.LocalhostSilo();

                    config.GatewayProvider = ClientConfiguration.GatewayProviderType.Custom;
                    config.CustomGatewayProviderAssemblyName = "Orleans.Clustering.DynamoDB";
                    config.ClusterId = "Cluster";

                    client = new ClientBuilder()
                        .UseConfiguration(config)
                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IHelloGrain).Assembly).WithReferences())
                        .ConfigureLogging(logging => logging.AddConsole())
                        .UseDynamoDBGatewayListProvider(options => {
                            LegacyDynamoDBGatewayListProviderConfigurator.ParseDataConnectionString(ConnectionString, options);
                        })
                        .Build();

                    await client.Connect();
                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (attempt > initializeAttemptsBeforeFailing)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
            }

            return client;
        }

        private static async Task DoClientWork(IClusterClient client)
        {
            var friend = client.GetGrain<IHelloGrain>(0);
            var response = await friend.SayHello("Good morning, my friend!");
            Console.WriteLine("\n\n{0}\n\n", response);
        }
    }
}