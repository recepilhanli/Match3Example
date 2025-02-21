using System.Threading;
using UnityEngine;

namespace Game.Utils
{

    public static class GlobalToken
    {
        private static CancellationTokenSource _tokenSource = null;
        public static CancellationToken token => _tokenSource.Token;
        public static void Refresh()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }
            _tokenSource = new CancellationTokenSource();
        }

        public static void Cancel()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }
        }
        
    }

}