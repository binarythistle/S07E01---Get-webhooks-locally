var builder = WebApplication.CreateBuilder(args);



var app = builder.Build();



app.UseHttpsRedirection();

app.MapPost("/webhooks", (HttpContext context) => {
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("--> We've got a hit");

    var headers = context.Request.Headers;

    foreach(var header in headers)
    {
        Console.WriteLine($"{header.Key} / {header.Value}");
    }
    Console.ResetColor();

    return Results.Ok();
});


app.Run();


