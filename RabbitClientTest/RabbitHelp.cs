using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitClientTest
{
    public interface IRabbitHelp
    {
        IModel Chanel { get; }
        ConnectionFactory Factory { get; }
        IConnection Connection { get; }
        void ReadMessage();
        void ReadMessage<T>();
    }

    class RabbitHelp : IRabbitHelp
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _chanel;
        internal RabbitHelp(ConnectionFactory factory)
        {
            _factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = Factory.CreateConnection();
            _chanel = Connection.CreateModel();
        }
        public ConnectionFactory Factory { get => _factory; }
        public IModel Chanel { get => _chanel; }
        public IConnection Connection { get => _connection; }
        public async void ReadMessage()
        { }
        public async void ReadMessage<T>()
        {
            Type t = typeof(T);
            IReadMessageStrategys ReadMessageOfTypeExchange = null;
            if (typeof(T) == typeof(Direct))
                ReadMessageOfTypeExchange = new ReadMessageDirect();
            if (typeof(T) == typeof(Topic))
                ReadMessageOfTypeExchange = new ReadMessageTopic();
            ReadMessageOfTypeExchange.ReadMessage(Factory, Chanel);
        }
        #region strategy of type exchange
        public interface IReadMessageStrategys
        {
            void ReadMessage(ConnectionFactory Factory, IModel Chanel);
        }
        private class ReadMessageDirect : IReadMessageStrategys
        {
            public void ReadMessage(ConnectionFactory Factory, IModel Chanel)
            {
                using (var connection = Factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    Chanel.ExchangeDeclare(exchange: "testmqex2topic", type: ExchangeType.Direct, autoDelete: true);

                    var queueName = Chanel.QueueDeclare().QueueName;

                    Chanel.QueueBind(queue: queueName,
                                             exchange: "testmqex2topic",
                                             routingKey: "router"
                                             );
                    var consumer = new EventingBasicConsumer(Chanel);
                    consumer.Received += (sender, e) =>
                    {
                        var body = e.Body;
                        var message = Encoding.UTF8.GetString(body.ToArray());
                        Console.WriteLine($"Received message: {message}, in direct key router");
                    };
                    Chanel.BasicConsume(queue: queueName,
                                        autoAck: true,
                                        consumer: consumer);
                    Console.ReadLine();
                }
            }
        }
        private class ReadMessageTopic : IReadMessageStrategys
        {
            public void ReadMessage(ConnectionFactory Factory, IModel Chanel)
            {
                using (var connection = Factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    Chanel.ExchangeDeclare(exchange: "testmqex", type: ExchangeType.Topic, autoDelete: true);

                    var queueName = Chanel.QueueDeclare().QueueName;
                    var topic = "rabbit.*.*";
                    Chanel.QueueBind(queue: queueName,
                                             exchange: "testmqex",
                                             routingKey: topic);
                    var consumer = new EventingBasicConsumer(Chanel);
                    consumer.Received += (sender, e) =>
                    {
                        var body = e.Body;
                        var message = Encoding.UTF8.GetString(body.ToArray());
                        Console.WriteLine(" Received message: {0}, in topic {1}", message, topic);
                    };

                    Chanel.BasicConsume(queue: queueName,
                                         autoAck: true,
                                         consumer: consumer);
                    Console.ReadLine();
                }
            }
        }
        #endregion
    }
    public class Direct
    { }
    public class Topic
    { }


}
