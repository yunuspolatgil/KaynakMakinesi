using System;
using System.Collections.Concurrent;
using KaynakMakinesi.Core.Logging;

namespace KaynakMakinesi.Infrastructure.Logging
{
    public sealed class InMemoryLogSink : ILogSink
    {
        private readonly int _capacity;
        private readonly ConcurrentQueue<LogEntry> _queue = new ConcurrentQueue<LogEntry>();

        public event EventHandler<LogEntry> EntryAdded;

        public InMemoryLogSink(int capacity)
        {
            _capacity = Math.Max(100, capacity);
        }

        public LogEntry[] Snapshot()
        {
            return _queue.ToArray();
        }

        public void Emit(LogEntry entry)
        {
            _queue.Enqueue(entry);

            // kapasite kontrolü (basit trim)
            while (_queue.Count > _capacity && _queue.TryDequeue(out _)) { }

            EntryAdded?.Invoke(this, entry);
        }
    }
}