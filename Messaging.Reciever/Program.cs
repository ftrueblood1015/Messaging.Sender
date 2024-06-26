using Azure.Messaging.ServiceBus;
using System;

namespace Messaging.Reciever
{
    internal class RecieverConsole
    {
        static string ConnectionString = "";
        static string QueueName = "";

        static async Task Main(string[] args)
        {
            var client = new ServiceBusClient(ConnectionString);

            var reciever = client.CreateReceiver(QueueName);

            Console.WriteLine("Recieving Messages...");

            while (true)
            {
                var message = await reciever.ReceiveMessageAsync();

                if (message != null)
                {
                    Console.Write(message.Body.ToString());

                    await reciever.CompleteMessageAsync(message);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("All Messages Recieved");
                    break;
                }
            }

            await reciever.CloseAsync();
        }
    }
}
