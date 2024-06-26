using Azure.Messaging.ServiceBus;

namespace Messaging.Sender
{
    internal class SenderConsole
    {
        static string ConnectionString = "";
        static string QueueName = "";

        static string Message = "Hello World!!";

        static async Task Main()
        {
            var client = new ServiceBusClient(ConnectionString);

            var sender = client.CreateSender(QueueName);

            Console.WriteLine("Sending Messages...");

            foreach(var character in Message)
            {
                var message = new ServiceBusMessage(character.ToString());
                await sender.SendMessageAsync(message);
                Console.WriteLine($"Sent    {character}");
            }

            await sender.CloseAsync();

            Console.WriteLine("Sent Messages.");
            Console.ReadLine();
        }
    }
}
