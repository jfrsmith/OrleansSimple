using System;
using System.Threading.Tasks;
using Orleans;

namespace GrainCollection
{
    public class HelloGrain : Grain, GrainInterfaces.IHelloGrain
    {
        public Task<string> SayHello(string name)
        {
            return Task.FromResult($"Hello, {name}!");
        }
    }
}
