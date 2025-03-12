namespace Infrastructure.Constants
{
    internal static class OpenTelConst
    {
        public static class Protocols
        {
            public const string Grpc = nameof(Grpc);
            public const string Http = nameof(Http);
        }

        public static class Exporters
        {
            public const string Oltp = nameof(Oltp);
            public const string Serilog = nameof(Serilog);
        }

    }
}
