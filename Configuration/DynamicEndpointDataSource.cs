using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace DynamicEndpoint.Configuration
{
    public class DynamicEndpointDataSource : EndpointDataSource
    {
        private readonly List<Endpoint> _endpoints = new();
        private readonly object _syncLock = new object();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

        public override IChangeToken GetChangeToken()
        {
            lock (_syncLock)
            {
                return new CancellationChangeToken(_cts.Token);
            }
        }

        public void AddEndpoint(Endpoint endpoint)
        {
            lock (_syncLock)
            {
                _endpoints.Add(endpoint);
            }
            RefreshChangeToken();
        }

        public void RemoveEndpoint(Endpoint endpoint)
        {
            RemoveEndpoint(endpoint.DisplayName!);
        }

        public void RemoveEndpoint(string path)
        {
            lock (_syncLock)
            {
                var removeItem = _endpoints.FirstOrDefault(x => x.DisplayName == path);
                if (removeItem != null)
                {
                    _endpoints.Remove(removeItem);
                    RefreshChangeToken();
                }
            }
        }

        private void RefreshChangeToken()
        {
            lock (_syncLock)
            {
                var oldCts = _cts;
                _cts = new CancellationTokenSource();
                oldCts.Cancel();
                oldCts.Dispose();
            }
        }
    }
}
