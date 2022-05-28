namespace SharpPulsar.TestContainer.TestUtils
{
    public sealed class DIContainer : IAsyncDisposable
    {
        private readonly Dictionary<Type, Dictionary<string, object>> _services;

        public DIContainer()
        {
            _services = new Dictionary<Type, Dictionary<string, object>>();
        }

        public void Register<T>(T service)
        {
            Register(service, string.Empty);
        }
        public void Register<T>(T service, string name)
        {
            if(service == null)
                throw new ArgumentNullException(nameof(service));

            if (!_services.ContainsKey(typeof(T)))
            {
                _services[typeof(T)] = new Dictionary<string, object>();
            }

            _services[typeof(T)][name] = service;
        }

        public T Get<T>()
        {
            return Get<T>(string.Empty);
        }

        public T Get<T>(string name)
        {
            return (T)_services[typeof(T)][name];
        }

        public async ValueTask DisposeAsync()
        {
            var services = _services.Values
                .Where(s => s != null && s.Any())
                .SelectMany(s => s.Values)
                .ToArray();

            foreach (var asyncDisp in services.Where(s => typeof(IAsyncDisposable).IsAssignableFrom(s.GetType())))
            {
                await ((IAsyncDisposable)asyncDisp).DisposeAsync();
            }

            foreach (var disposable in services.Where(s => typeof(IDisposable).IsAssignableFrom(s.GetType())))
            {
                ((IDisposable)disposable).Dispose();
            }
        }

        public static DIContainer Default { get; private set; } = new DIContainer();
    }
}
