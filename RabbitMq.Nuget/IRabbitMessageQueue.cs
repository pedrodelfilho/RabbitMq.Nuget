using System;
using System.Threading.Tasks;

namespace RabbitMq.Nuget
{
    public delegate Task CallbackEvent(object obj, EventArgs e);
    public interface IRabbitMessageQueue
    {
        CallbackEvent Ack { get; set; }

        CallbackEvent Nack { get; set; }

        CallbackEvent SendFailed { get; set; }

        void SubscribeWithConfirmation(Func<string, bool> action);

        void PutWithConfirmation(string message);
    }
}
