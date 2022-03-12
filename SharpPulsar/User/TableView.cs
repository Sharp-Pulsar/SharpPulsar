using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Interfaces;

namespace SharpPulsar.User
{
    public class TableView<T> : ITableView<T>
    {
        private readonly IActorRef _tableView;
        public TableView()
        {

        }
        public bool Empty => throw new NotImplementedException();

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ISet<KeyValuePair<string, T>> EntrySet()
        {
            throw new NotImplementedException();
        }

        public void ForEach(Action<string, T> action)
        {
            throw new NotImplementedException();
        }

        public void ForEachAndListen(Action<string, T> action)
        {
            throw new NotImplementedException();
        }

        public T Get(string key)
        {
            throw new NotImplementedException();
        }

        public ISet<string> KeySet()
        {
            throw new NotImplementedException();
        }

        public int Size()
        {
            throw new NotImplementedException();
        }

        public ICollection<T> Values()
        {
            throw new NotImplementedException();
        }
    }
}
