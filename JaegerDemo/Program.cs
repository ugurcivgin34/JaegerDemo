using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OpenTelemetry servislerini eklemek için kullanýlýr. Bu, uygulamanýzýn izleme verilerini toplamak için gerekli hizmetleri yapýlandýrýr.
builder.Services.AddOpenTelemetry()
    // Kaynak bilgilerini yapýlandýrýr. Burada "JaegerDemoService" adýnda bir hizmet eklenir. Bu, izleme verilerinin hangi hizmete ait olduðunu belirtir.
    .ConfigureResource(resourceBuilder => resourceBuilder.AddService("JaegerDemoService"))
    // Ýzleme (tracing) yapýlandýrmasýný baþlatýr. Bu, izleme saðlayýcýsýnýn nasýl davranacaðýný belirler.
    .WithTracing(tracerProviderBuilder =>
    {
        // ASP.NET Core uygulamasý için izleme (instrumentation) ekler. Bu, gelen HTTP isteklerini otomatik olarak izlemeye baþlar.
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            // HTTP istemcileri için izleme ekler. Bu, uygulamanýzýn yaptýðý dýþ HTTP isteklerini izlemeye baþlar.
            .AddHttpClientInstrumentation()
            // Jaeger izleme verilerini göndermek için bir ihracatçý ekler. Bu, izleme verilerinin Jaeger'e gönderilmesini saðlar.
            .AddJaegerExporter(o =>
            {
                // Jaeger Agent'ýn çalýþtýðý ana bilgisayarýn adýný belirtir. Burada "localhost" kullanýlmýþ, yani Jaeger Agent yerel makinede çalýþýyor.
                o.AgentHost = "localhost";
                // Jaeger Agent'ýn dinlediði portu belirtir. Burada standart Jaeger portu olan 6831 kullanýlmýþ.
                o.AgentPort = 6831;
            });
    })
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });


var app = builder.Build();
// Prometheus metriklerinin toplanmasý için endpoint eklenir
app.UseOpenTelemetryPrometheusScrapingEndpoint("/metrics");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
