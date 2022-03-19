using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Interfaces;
using SharpPulsar.Table.Messages;

namespace SharpPulsar.Table
{
    public class TableView<T> : ITableView<T>
    {
        private readonly ConcurrentDictionary<string, T> _data;
        private IActorRef _tableViewActor;
        public TableView(IActorRef tableViewActor, ConcurrentDictionary<string, T> data)
        {
            _tableViewActor = tableViewActor;  
            _data = data;   
        }
        public async Task CloseAsync()
        {
            await _tableViewActor.GracefulStop(TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            _tableViewActor.GracefulStop(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        }

        public void ForEachAndListen(Action<string, T> action)
        {
            _tableViewActor.Tell(new ForEachAction<T>(action));
        }

        public virtual int Size()
        {
            return _data.Count();
        }

        public virtual bool Empty
        {
            get
            {
                return _data.Count() == 0;
            }
        }

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public virtual T Get(string key)
        {
            _data.TryGetValue(key, out var v);
            return v;
        }

        public ISet<KeyValuePair<string, T>> EntrySet()
        {
            return _data.Select(kv => new KeyValuePair<string, T>(kv.Key, kv.Value)).ToHashSet();
        }

        public virtual ISet<string> KeySet()
        {
            return _data.Keys.ToHashSet();
        }

        public virtual ICollection<T> Values()
        {
            return _data.Values;
        }

    }
}
