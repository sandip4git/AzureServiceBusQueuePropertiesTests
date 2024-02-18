using Azure.Messaging.ServiceBus;
using System.Text;

namespace Azure_Service_Bus_Demo
{
    internal class Program
    {
        const string ServiceBusConnectionString = "your-service-bus-connnection-with-send-policy";
        const string QueueName = "your-queue-name";
      
        static async Task Main(string[] args)
        {
            // Create a client for the queue
            var client = new ServiceBusClient(ServiceBusConnectionString);
            var sender = client.CreateSender(QueueName);

            try
            {
                // Create a sample message
                string messageBody = "Hello, world!";
                var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

                // Send the message to the queue
                await sender.SendMessageAsync(message);

                Console.WriteLine($"Message sent: {messageBody}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }

            // Close the client after sending the message
            await client.DisposeAsync();
        }
    }
}
