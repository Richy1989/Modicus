using System.Threading;
using Modicus.Manager.Interfaces;

namespace Modicus.Manager
{
    internal class TokenManager : ITokenManager
    {
        public CancellationToken Token { get; }
        private readonly CancellationTokenSource source;

        /// <summary>Initializes a new instance of the <see cref="TokenManager"/> class.</summary>
        public TokenManager()
        {
            source = new();
            Token = source.Token;
        }
    }
}