using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets(typeof(Program).Assembly, optional: true)
    .Build();


string connectionString = configuration["ServiceBusConnectionString"]!;
string topicName = configuration["ServiceBusTopicName"]!;
string subscriptionName = configuration["ServiceBusSubscriptionName"]!;

ServiceBusClient client;
ServiceBusProcessor processor;

client = new ServiceBusClient(connectionString);

processor = client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());

try
{
    processor.ProcessMessageAsync += MessageHandler;
    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();
    Console.WriteLine("--> Listening to Service Bus");

    Console.WriteLine("Press any key to end processing");
    Console.ReadKey();

    Console.WriteLine("--> Attempting to stop receiver");
    await processor.StopProcessingAsync();
    Console.WriteLine("--> Receiever stopped.");
}
finally
{
    await processor.DisposeAsync();
    await client.DisposeAsync();
}



// Handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();

    var wheDto = JsonConvert.DeserializeObject<WebhookEventDto>(body);

    Console.WriteLine($"--> Headers: {wheDto!.Headers} / Body: {wheDto.Body}");

    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}