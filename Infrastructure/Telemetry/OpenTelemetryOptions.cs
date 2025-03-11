using Domain.Core.SharedKernel;

namespace Infrastructure.Telemetry
{
    public sealed class OpenTelemetryOptions : IAppOptions
    {
        static string IAppOptions.ConfigSectionPath => nameof(OpenTelemetryOptions);
        public string ServiceName { get; init; } = "OpenTelemetry_Default";
        public TracingOptions Tracing { get; init; } = new();
        public MetricsOptions Metrics { get; init; } = new();
        public LoggingOptions Logging { get; init; } = new();
    }

    public sealed class TracingOptions
    {
        public bool Enabled { get; set; }
        public string Exporter { get; set; } = "Otlp";
        public OtlpOptions Otlp { get; set; } = new();
        public double SampleRate { get; set; } = 1.0;
    }

    public sealed class MetricsOptions
    {
        public bool Enabled { get; set; }
        public string Exporter { get; set; } = "Otlp";
        public OtlpOptions Otlp { get; set; } = new();
    }

    public sealed class LoggingOptions
    {
        public bool Enabled { get; set; }
        public Dictionary<string, string> LogLevel { get; set; } = new();
        public string Exporter { get; set; } = "Otlp";
        public OtlpOptions Otlp { get; set; } = new();
    }

    public sealed class OtlpOptions
    {
        public string Endpoint { get; set; } = "http://localhost:4317";
        public string Protocol { get; set; } = "grpc";
    }
}
