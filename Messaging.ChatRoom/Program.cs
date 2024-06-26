using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using System;

namespace Messaging.ChatRoom
{
    internal class ChatRoom
    {
        static string ConnectionString = "";
        static string TopicName = "";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter name:");
            var userName = Console.ReadLine();


            // Create an administration client to manage artifacts
            var serviceBusAdministrationClient = new ServiceBusAdministrationClient(ConnectionString);

            // Create a topic if it does not exist
            if (!await serviceBusAdministrationClient.TopicExistsAsync(TopicName))
            {
                await serviceBusAdministrationClient.CreateTopicAsync(TopicName);
            }

            // Create a temporary subscription for the user if it does not exist
            if (!await serviceBusAdministrationClient.SubscriptionExistsAsync(TopicName, userName))
            {
                var options = new CreateSubscriptionOptions(TopicName, userName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
                };
                await serviceBusAdministrationClient.CreateSubscriptionAsync(options);
            }

            var clientOptions = new ServiceBusClientOptions();
            clientOptions.RetryOptions = new ServiceBusRetryOptions
            {
                Delay = TimeSpan.FromSeconds(10),
                MaxDelay = TimeSpan.FromSeconds(30),
                Mode = ServiceBusRetryMode.Exponential,
                MaxRetries = 3,
            };

            // Create a service bus client
            var serviceBusClient = new ServiceBusClient(ConnectionString, clientOptions);

            // Create a service bus sender
            var serviceBusSender = serviceBusClient.CreateSender(TopicName);

            // Create a message processor
            var processor = serviceBusClient.CreateProcessor(TopicName, userName);

            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;


            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;



            await processor.StartProcessingAsync();

            await serviceBusSender.SendMessageAsync(new ServiceBusMessage($"{userName} has entered the room."));


            while (true)
            {
                var text = Console.ReadLine();

                if (text == "exit")
                {

                    break;
                }

                await serviceBusSender.SendMessageAsync(new ServiceBusMessage($"{userName}: {text}"));

            }

            await serviceBusSender.SendMessageAsync(new ServiceBusMessage($"{userName} has left the room."));
            await processor.StopProcessingAsync();


            await processor.CloseAsync();
            await serviceBusSender.CloseAsync();


        }

        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            Console.WriteLine(args.Message.Body.ToString());
            await args.CompleteMessageAsync(args.Message);
        }

        static async Task ErrorHandler(ProcessErrorEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}