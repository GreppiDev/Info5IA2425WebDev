using System.Diagnostics;

namespace EducationalGames.Services;

/// <summary>
/// Servizio di esempio per dimostrare l'uso di OpenTelemetry tracing con configurazione dinamica
/// </summary>
public class TelemetryDemoService : IDisposable
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger<TelemetryDemoService> _logger;
    private readonly string _serviceName;
    private readonly string _serviceVersion; public TelemetryDemoService(ILogger<TelemetryDemoService> logger, IConfiguration configuration)
    {
        _logger = logger;

        // Legge le variabili d'ambiente OTEL per configurare l'ActivitySource
        _serviceName = configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? "EducationalGames";
        _serviceVersion = configuration.GetValue<string>("OTEL_SERVICE_VERSION") ?? "1.0.0";

        // Crea l'ActivitySource usando il nome del servizio dalle variabili d'ambiente
        var activitySourceName = $"{_serviceName}.Demo";
        _activitySource = new ActivitySource(activitySourceName, _serviceVersion);

        _logger.LogInformation("TelemetryDemoService initialized with ActivitySource: {ActivitySourceName} v{Version}",
            activitySourceName, _serviceVersion);
    }

    /// <summary>
    /// Metodo di esempio che crea una traccia personalizzata
    /// </summary>
    public async Task<string> ProcessDataAsync(string inputData)
    {
        using var activity = _activitySource.StartActivity("ProcessData");
        activity?.SetTag("input.data", inputData);
        activity?.SetTag("operation.type", "data_processing");
        activity?.SetTag("service.name", _serviceName);
        activity?.SetTag("service.version", _serviceVersion);

        try
        {
            _logger.LogInformation("Starting data processing for input: {InputData}", inputData);

            // Simula operazioni di elaborazione
            await Step1ValidationAsync(inputData);
            var processedData = await Step2TransformationAsync(inputData);
            await Step3PersistenceAsync(processedData);

            activity?.SetTag("result.status", "success");
            activity?.SetTag("output.length", processedData.Length);

            _logger.LogInformation("Data processing completed successfully");
            return processedData;
        }
        catch (Exception ex)
        {
            activity?.SetTag("result.status", "error");
            activity?.SetTag("error.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(ex, "Error processing data");
            throw;
        }
    }

    private async Task Step1ValidationAsync(string data)
    {
        using var activity = _activitySource.StartActivity("Step1.Validation");
        activity?.SetTag("step", "validation");

        await Task.Delay(50); // Simula elaborazione

        if (string.IsNullOrEmpty(data))
        {
            throw new ArgumentException("Input data cannot be null or empty");
        }

        activity?.SetTag("validation.result", "passed");
    }

    private async Task<string> Step2TransformationAsync(string data)
    {
        using var activity = _activitySource.StartActivity("Step2.Transformation");
        activity?.SetTag("step", "transformation");
        activity?.SetTag("input.length", data.Length);

        await Task.Delay(100); // Simula elaborazione

        var transformed = $"PROCESSED_{data.ToUpper()}_{DateTime.UtcNow:yyyyMMddHHmmss}";

        activity?.SetTag("output.length", transformed.Length);
        activity?.SetTag("transformation.type", "uppercase_with_timestamp");

        return transformed;
    }

    private async Task Step3PersistenceAsync(string data)
    {
        using var activity = _activitySource.StartActivity("Step3.Persistence");
        activity?.SetTag("step", "persistence");
        activity?.SetTag("data.length", data.Length);

        await Task.Delay(75); // Simula salvataggio

        activity?.SetTag("persistence.result", "saved");
        activity?.SetTag("persistence.location", "memory");
    }    /// <summary>
         /// Metodo che genera metriche personalizzate con informazioni dalle variabili OTEL
         /// </summary>
    public void RecordCustomMetrics(string operation, double duration, bool success)
    {
        // Le metriche possono essere registrate tramite OpenTelemetry Metrics
        // Questo è un esempio di come si potrebbero strutturare utilizzando le variabili OTEL
        _logger.LogInformation("Recording custom metrics: Service={ServiceName} v{ServiceVersion}, Operation={Operation}, Duration={Duration}ms, Success={Success}",
            _serviceName, _serviceVersion, operation, duration, success);

        // Qui si potrebbero aggiungere metriche personalizzate usando System.Diagnostics.Metrics
        // con le informazioni dal servizio configurato tramite variabili d'ambiente
    }

    /// <summary>
    /// Rilascia le risorse utilizzate dall'ActivitySource
    /// </summary>
    public void Dispose()
    {
        _activitySource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
