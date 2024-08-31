using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using Akka.Util.Internal;
using SharpPulsar.Interfaces;
using SharpPulsar.Table.Messages;

namespace SharpPulsar.Table
{
    public class TableView<T> : ITableView<T>
    {
        private IActorRef _tableViewActor;
        public TableView(IActorRef tableViewActor)
        {
            _tableViewActor = tableViewActor;  
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
            try
            {
                var data = _tableViewActor.Ask<ImmutableDictionary<string, T>>(TableData.Instance).GetAwaiter().GetResult();
                data.ForEach(kv => action(kv.Key, kv.Value));
                _tableViewActor.Tell(action);
            }
            finally { }
           
        }
        
        public virtual int Size()
        {            
            //return _data.Count();
            return _tableViewActor.Ask<int>(TableDataSize.Instance).GetAwaiter().GetResult();
        }

        public virtual bool Empty
        {
            get
            {
                //return _data.Count() == 0;
                return _tableViewActor.Ask<bool>(TableDataEmpty.Instance).GetAwaiter().GetResult();
            }
        }

        public bool ContainsKey(string key)
        {
            //TableDataKey
            //return _data.ContainsKey(key);
            return _tableViewActor.Ask<bool>(new TableDataKey(key)).GetAwaiter().GetResult();
        }

        public virtual T Get(string key)
        {
            return _tableViewActor.Ask<T>(new TableDataGet(key)).GetAwaiter().GetResult();
        }

        public ISet<KeyValuePair<string, T>> EntrySet()
        {
            return _tableViewActor.Ask<ISet<KeyValuePair<string, T>>>(TableDataEntrySet.Instance).GetAwaiter().GetResult();
            //return _data.Select(kv => new KeyValuePair<string, T>(kv.Key, kv.Value)).ToHashSet();
        }

        public virtual ISet<string> KeySet()
        {
            
            return _tableViewActor.Ask<ISet<string>>(TableDataKeySet.Instance).GetAwaiter().GetResult();
            //return _data.Keys.ToHashSet();
        }

        public virtual ICollection<T> Values()
        {
            return _tableViewActor.Ask<ICollection<T>>(TableDataValues.Instance).GetAwaiter().GetResult();
        }

        public async ValueTask RefreshAsync()
        {
            //RefeshData
            var s = await _tableViewActor.Ask<bool>(RefeshData.Instance);
            //throw new NotImplementedException();
        }

    }
    public class AskTable<T>
    {
        
    }
}
