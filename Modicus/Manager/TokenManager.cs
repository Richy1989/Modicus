using System.Threading;
using Modicus.Manager.Interfaces;

namespace Modicus.Manager
{
    internal class TokenManager : ITokenManager
    {
        private readonly CancellationTokenSource source;

        public TokenManager()
        {
            source = new();
            Token = source.Token;
        }

        public CancellationToken Token { get; }
    }
}