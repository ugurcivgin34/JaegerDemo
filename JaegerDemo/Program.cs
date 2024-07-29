using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OpenTelemetry servislerini eklemek i�in kullan�l�r. Bu, uygulaman�z�n izleme verilerini toplamak i�in gerekli hizmetleri yap�land�r�r.
builder.Services.AddOpenTelemetry()
    // Kaynak bilgilerini yap�land�r�r. Burada "JaegerDemoService" ad�nda bir hizmet eklenir. Bu, izleme verilerinin hangi hizmete ait oldu�unu belirtir.
    .ConfigureResource(resourceBuilder => resourceBuilder.AddService("JaegerDemoService"))
    // �zleme (tracing) yap�land�rmas�n� ba�lat�r. Bu, izleme sa�lay�c�s�n�n nas�l davranaca��n� belirler.
    .WithTracing(tracerProviderBuilder =>
    {
        // ASP.NET Core uygulamas� i�in izleme (instrumentation) ekler. Bu, gelen HTTP isteklerini otomatik olarak izlemeye ba�lar.
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            // HTTP istemcileri i�in izleme ekler. Bu, uygulaman�z�n yapt��� d�� HTTP isteklerini izlemeye ba�lar.
            .AddHttpClientInstrumentation()
            // Jaeger izleme verilerini g�ndermek i�in bir ihracat�� ekler. Bu, izleme verilerinin Jaeger'e g�nderilmesini sa�lar.
            .AddJaegerExporter(o =>
            {
                // Jaeger Agent'�n �al��t��� ana bilgisayar�n ad�n� belirtir. Burada "localhost" kullan�lm��, yani Jaeger Agent yerel makinede �al���yor.
                o.AgentHost = "localhost";
                // Jaeger Agent'�n dinledi�i portu belirtir. Burada standart Jaeger portu olan 6831 kullan�lm��.
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
// Prometheus metriklerinin toplanmas� i�in endpoint eklenir
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
