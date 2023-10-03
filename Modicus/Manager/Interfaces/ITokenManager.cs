using System.Threading;

namespace Modicus.Manager.Interfaces
{
    internal interface ITokenManager
    {
        CancellationToken Token { get; }
    }
}