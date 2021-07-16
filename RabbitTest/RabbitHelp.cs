using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitTest
{
    public interface IRabbitHelp
    {
        IModel Chanel { get; }
        ConnectionFactory Factory { get; }
        IConnection Connection { get; }
        Func<Task> SendMessage<T>(string mes);
    }
    class RabbitHelp : IRabbitHelp
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _chanel;
        internal RabbitHelp(ConnectionFactory factory)
        {
            _factory = factory;
            _connection = Factory.CreateConnection();
            _chanel = Connection.CreateModel();
        }
        public ConnectionFactory Factory { get => _factory; }
        public IModel Chanel { get => _chanel; }
        public IConnection Connection { get => _connection; }
        public Func<Task> SendMessage<T>(string mes)
        {
            Type t = typeof(T);
            ISendMessageStrategys ReadMessageOfTypeExchange = null;
            if (typeof(T) == typeof(Direct))
                ReadMessageOfTypeExchange = new SendMessageDirect();
            if (typeof(T) == typeof(Topic))
                ReadMessageOfTypeExchange = new SendMessageTopic();
            return ReadMessageOfTypeExchange.SendMessage(Connection, Chanel, mes);
        }
        #region strategy of type exchange
        public interface ISendMessageStrategys
        {
            public Func<Task> SendMessage(IConnection Connection, IModel Chanel, string mes);
        }
        private class SendMessageDirect : ISendMessageStrategys
        {
            public Func<Task> SendMessage(IConnection Connection, IModel Chanel, string mes)
            {
                return async () =>
                {
                    using (Connection)
                    {
                        using (Chanel)
                        {
                            do
                            {
                                Chanel.ExchangeDeclare("testmqex2topic", type: ExchangeType.Direct, autoDelete: true);
                                var @byte = Encoding.UTF8.GetBytes(mes);

                                Chanel.BasicPublish(
                                                  exchange: "testmqex2topic",
                                                  routingKey: "router",
                                                  basicProperties: null,
                                                  body: @byte
                                                  );
                                Console.WriteLine("publish in direct key router");
                                await Task.Delay(3000);
                            }
                            while (true);
                        }
                    }
                };

            }
        }
        private class SendMessageTopic : ISendMessageStrategys
        {
            public Func<Task> SendMessage(IConnection Connection, IModel Chanel, string mes)
            {
                return async () =>
                {
                    using (Connection)
                    {
                        using (Chanel)
                        {
                            do
                            {
                                Chanel.ExchangeDeclare("testmqex", type: ExchangeType.Topic, autoDelete: true);
                                var routingKey = "rabbit.mq.test";
                                var @byte = Encoding.UTF8.GetBytes(mes);
                                Console.WriteLine($"publish in {routingKey}");
                                Chanel.BasicPublish(
                                                  exchange: "testmqex",
                                                  routingKey: routingKey,
                                                  basicProperties: null,
                                                  body: @byte
                                                  );
                                await Task.Delay(3000);
                            }
                            while (true);
                        }
                    }
                };
            }
            #endregion
        }
    }
    public class Direct
    { }
    public class Topic
    { }
}
