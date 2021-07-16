using System;
using System.Threading.Tasks;
using Unity;

namespace RabbitTest
{
    class Program
    {
        static UnityContainer Container;
        static IRabbitHelp RabbitMq;
        static async Task<int> Main(string[] args)
        {
            var access = Settings();
            if (!access)
                return 1;
            var accessByTopic = Task.Run(RabbitMq.SendMessage<Topic>("test"));
            var accessByDirect = Task.Run(RabbitMq.SendMessage<Direct>("test"));
            await Task.WhenAll(accessByTopic, accessByDirect);

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
