
using FussballNews.ai.Spider;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(
    appInsightsConnectionString: builder.Configuration["ApplicationInsights:ConnectionString"],
    appConfigEndpoint: builder.Configuration["AzureAppConfig:EndpointUrl"]
);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddRebus(
    serviceBusConnectionString: builder.Configuration.GetConnectionString("ServiceBus")!,
    databaseConnectionString: builder.Configuration.GetConnectionString("DbContext")!,
    queueName: "spider" ,
    onCreated: FussballNewsAiSpiderServiceExtension.AddRebusSubscriptions
);
builder.AddAzureStorage(builder.Configuration.GetConnectionString("AzureStorage")!);
builder.AddDbContext();
builder.Services.AddFussballNewsAiSpider();

builder.Host.AddSerilog();

    builder.AddFussballNewsOpenAi(options =>
    {
        options.ApiKey = builder.Configuration["OpenAI:ApiKey"];
        options.Endpoint = builder.Configuration["OpenAI:Endpoint"];
        options.ChatModelName = builder.Configuration["OpenAI:ChatModelName"];
        options.EmbeddingModelName = builder.Configuration["OpenAI:EmbeddingModelName"];
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// Move app.UseCors("AllowAll"); to just after app.UseHttpsRedirection() and before any authentication/authorization middleware.

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAll"); 

if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
