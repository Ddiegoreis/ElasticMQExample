using Amazon.SQS;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var serviceUrl = builder.Configuration["Sqs:ServiceUrl"] ?? "http://localhost:9324";
var region = builder.Configuration["Sqs:Region"] ?? "us-east-1";
var queueName = builder.Configuration["Sqs:QueueName"] ?? "minha-nova-fila";

// Add controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Amazon SQS client
builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var config = new Amazon.SQS.AmazonSQSConfig
    {
        ServiceURL = serviceUrl,
        AuthenticationRegion = region
    };
    return new Amazon.SQS.AmazonSQSClient("dummy", "dummy", config);
});

// Queue URL provider that ensures the queue exists
builder.Services.AddSingleton<QueueUrlProvider>(sp => new QueueUrlProvider(sp.GetRequiredService<IAmazonSQS>(), queueName));

// Producer service
builder.Services.AddScoped<IMessageProducer, SqsMessageProducer>();

// Background consumer hosted service
builder.Services.AddHostedService<SqsConsumerHostedService>();

var app = builder.Build();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
