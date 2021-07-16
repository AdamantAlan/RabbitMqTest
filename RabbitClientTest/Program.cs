using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace RabbitClientTest
{
    class Program
    {
        static UnityContainer Container;
        static IRabbitHelp RabbitMq;
        static async Task<int> Main(string[] args)
        {
            var ret = Settings();
            if (!ret)
                return 1;
            var accessByTopic = Task.Run(() => RabbitMq.ReadMessage<Topic>());
            var accessByDirect = Task.Run(() => RabbitMq.ReadMessage<Direct>());
            await Task.WhenAll(accessByTopic, accessByDirect);
            //  await Task.WhenAll(accessByTopic);
            return 0;
        }
        private static bool Settings()
        {
            try
            {
                Container = new UnityContainer();
                Container.RegisterType<IRabbitHelp, RabbitHelp>("ch1");
                RabbitMq = Container.Resolve<RabbitHelp>("ch1");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}

