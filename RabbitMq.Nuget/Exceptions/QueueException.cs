using System;
using System.Collections.Concurrent;

namespace RabbitMq.Nuget.Exceptions
{
    public class QueueException : Exception
    {
        private object _message;
        private ConcurrentDictionary<ulong, string> _dicio;

        public QueueException(Exception inner) : base("", inner) { }

        public QueueException(Exception inner, string obj) : base("", inner) { _message = obj; }

        public QueueException(Exception inner, ConcurrentDictionary<ulong, string> obj) : base("", inner) { _dicio = obj; }

        public string ObterMensagem => (string)_message;
        public ConcurrentDictionary<ulong, string> ObterDicionario => _dicio;
    }
}

