using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public class HttpTrigger1
    {
        static string connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString")!;
        static string serviceBusTopic = Environment.GetEnvironmentVariable("ServiceBusTopic")!;

        static ServiceBusClient? client;
        static ServiceBusSender? sender;

        private readonly ILogger<HttpTrigger1> _logger;

        public HttpTrigger1(ILogger<HttpTrigger1> logger)
        {
            _logger = logger;
        }

        [Function("HttpTrigger1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            WebhookEventDto webhookEvent = new WebhookEventDto(JsonConvert.SerializeObject(req.Headers), await new StreamReader(req.Body).ReadToEndAsync());


            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(serviceBusTopic);

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            if(!messageBatch.TryAddMessage(new ServiceBusMessage(JsonConvert.SerializeObject(webhookEvent))))
            {
                throw new Exception("The message is too large to fit into the batch");
            }

            try
            {
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine("A batch has been placed onto the message bus topic");
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
