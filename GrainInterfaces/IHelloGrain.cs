using System;
using System.Threading.Tasks;
using Orleans;

namespace GrainInterfaces
{
    public interface IHelloGrain : IGrainWithIntegerKey
    {
        Task<string> SayHello(string name);
    }
}
