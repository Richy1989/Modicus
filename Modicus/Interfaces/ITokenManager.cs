using System.Threading;

namespace Modicus.Interfaces
{
    internal interface ITokenManager
    {
        CancellationToken Token { get; }
    }
}