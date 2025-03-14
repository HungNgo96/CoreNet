namespace Infrastructure.Constants
{
    public static class OpenTelConst
    {
        internal static class Protocols
        {
            public const string Grpc = nameof(Grpc);
            public const string Http = nameof(Http);
        }

        internal static class Exporters
        {
            public const string Oltp = nameof(Oltp);
            public const string Serilog = nameof(Serilog);
        }

        public static class MetricNames
        {
            private const string Suffix = "CoreApp.{0}.Metrics";

            public static class Products
            {
                public static readonly string OpenTelScopeName = string.Format(Suffix, nameof(Products));
                public const string Get = "app_get_production_total";
                public const string GetById = "app_get_by_id_total";
            }
        }
    }
}
