using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Xunit;

namespace QueuePropertiesTest
{
    public class QueuePropertiesTest
    {

        // Connection string for your Service Bus namespace
        const string connectionString = "your-service-bus-connnection-at-root-level-with-send-receive-policy";

        // Name of your queue
        const string queueName = "your-queu-name";

        // Create a ServiceBusAdministrationClient to manage queue properties
        ServiceBusAdministrationClient adminClient = new ServiceBusAdministrationClient(connectionString);

        // Create a ServiceBusClient to send and receive messages
        ServiceBusClient client = new ServiceBusClient(connectionString);

        // Test the default time to live property of the queue
        [Fact]
        public async Task TestDefaultTimeToLive()
        {
            // Get the queue properties
            QueueProperties queueProperties = await adminClient.GetQueueAsync(queueName);

            // Assert that the default time to live is 14 days
            Assert.Equal(TimeSpan.FromMinutes(1), queueProperties.DefaultMessageTimeToLive);
        }

        // Test the dead lettering on message expiration property of the queue
        [Fact]
        public async Task TestDeadLetteringOnMessageExpiration()
        {
            // Get the queue properties
            QueueProperties queueProperties = await adminClient.GetQueueAsync(queueName);

            // Assert that the dead lettering on message expiration is enabled
            Assert.True(queueProperties.DeadLetteringOnMessageExpiration);
        }

        // Test the message expiration and dead lettering behavior of the queue
        [Fact]
        public async Task TestMessageExpirationAndDeadLettering()
        {
            // Create a sender for the queue
            ServiceBusSender sender = client.CreateSender(queueName);

            // Create a receiver for the queue
            ServiceBusReceiver receiver = client.CreateReceiver(queueName);

            // Create a receiver for the dead letter queue
            ServiceBusReceiver deadLetterReceiver = client.CreateReceiver("nonprod-dev-queue1/$DeadLetterQueue");

            // Create a message with a short time to live
            ServiceBusMessage message = new ServiceBusMessage("Hello, world!");
            message.TimeToLive = TimeSpan.FromSeconds(5);

            // Send the message to the queue
            await sender.SendMessageAsync(message);

            // Wait for the message to expire
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Try to receive the message from the queue
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));

            // Assert that the message is null, as it has expired
            Assert.Null(receivedMessage);

            // Receive the message from the dead letter queue
            ServiceBusReceivedMessage deadLetterMessage = await deadLetterReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));

            // Assert that the message is not null, as it has been dead lettered
            Assert.NotNull(deadLetterMessage);

            // Assert that the message body is the same as the original message
            Assert.Equal("Hello, world!", deadLetterMessage.Body.ToString());
        }
    }
}
