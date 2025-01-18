using RabbitMq.Nuget.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Nuget
{
    public class RabbitMessageQueue : IRabbitMessageQueue
    {
        private IConnection connection;
        private IModel channel;

        public string Servidor { get; set; }
        public string VHost { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public string Exchange { get; set; }
        public bool MultiplosCanais { get; set; }
        public string Fila { get; set; }

        private string nomeFila;
        private string exchange;

        public CallbackEvent Ack { get; set; }

        public CallbackEvent Nack { get; set; }

        public CallbackEvent SendFailed { get; set; }

        public Dictionary<string, IModel> Canais { get; set; }

        public RabbitMessageQueue(string nomeServidor, string vhost, string usuario, string senha,
             string exchange, bool multiplosCanais, string nomeFila)
        {
            Initialize(nomeServidor, vhost, usuario, senha, multiplosCanais, nomeFila, exchange);
        }

        public void PutWithConfirmation(string message)
        {
            try
            {
                channel.ConfirmSelect();

                var props = channel.CreateBasicProperties();
                props.DeliveryMode = 2;

                channel.BasicPublish(exchange: exchange,
                                    routingKey: nomeFila,
                                    basicProperties: props,
                                    body: Encoding.UTF8.GetBytes(message),
                                    mandatory: true);

                channel.BasicAcks += async (sender, e) => await Channel_BasicAcks(message, e);
                channel.BasicNacks += async (sender, e) => await Channel_BasicNacks(message, e);
                channel.BasicReturn += async (sender, e) => await Channel_BasicReturn(message, e);

                channel.WaitForConfirmsOrDie(new TimeSpan(0, 1, 0));
            }
            catch (Exception e)
            {
                throw new QueueException(e, message);
            }
        }

        private async Task Channel_BasicAcks(object sender, BasicAckEventArgs e) => await Ack.Invoke(sender, e);
        private async Task Channel_BasicNacks(object sender, BasicNackEventArgs e) => await Nack.Invoke(sender, e);
        private async Task Channel_BasicReturn(object sender, BasicReturnEventArgs e) => await SendFailed.Invoke(sender, e);

        public void SubscribeWithConfirmation(Func<string, bool> action)
        {
            var message = string.Empty;
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body;
                    message = Encoding.UTF8.GetString(body.ToArray());

                    var isSuccess = ExecuteAction(message, action);

                    if (isSuccess)
                        channel.BasicAck(ea.DeliveryTag, false);
                    else
                        channel.BasicNack(ea.DeliveryTag, false, true);
                }
                catch (Exception e)
                {
                    channel.BasicNack(ea.DeliveryTag, true, false);
                    throw new QueueException(e);
                }
            };

            channel.BasicConsume(queue: nomeFila, autoAck: false, consumer: consumer);
        }

        public void Initialize(string nomeServidor, string vhost, string usuario, string senha, bool multiplosCanais, string fila, string nomeExchange)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = nomeServidor,
                    UserName = usuario,
                    Password = senha,
                    VirtualHost = vhost
                };

                connection = factory.CreateConnection();
                exchange = nomeExchange;
                if (!multiplosCanais)
                {
                    channel = connection.CreateModel();
                    nomeFila = fila;
                }
                else
                    Canais = [];
            }
            catch (Exception e)
            {
                throw new QueueException(e);
            }
        }

        private static bool ExecuteAction(string message, Func<string, bool> ExecutionMethod)
        {
            if (ExecutionMethod == null)
                throw new ArgumentNullException(nameof(ExecutionMethod));

            return ExecutionMethod(message);
        }

        /// <summary>
        /// Ver <see cref="IDisposable.Dispose"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Ver <see cref="IDisposable.Dispose()"/>
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                connection.Dispose();
            }
        }
    }
}
