using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Common.Extensions
{
    internal static class EventIds
    {
        public const int Request = 11;
        public const int ErrorResult = 12;
        public const int HttpErrorResult = 13;
        public const int Response = 14;
        public const int Error = 15;
        public const int Info = 16;
        public const int Warning = 17;
        public const int Debug = 18;
        public const int ErrorMediatrResult = 19;
        public const int Add = 101;
        public const int Added = 102;
        public const int Update = 201;
        public const int Updated = 202;
        public const int Delete = 301;
        public const int Deleted = 302;
        public const int Job = 401;
        public const int JobError = 402;
        public const int Grpc = 501;
        public const int GrpcError = 502;
    }

    public static class LoggerExtensions
    {
        private static readonly JsonSerializerOptions s_jsonSerializerOption = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        private static readonly Action<ILogger, string, string, string, string, Exception?> s_request = LoggerMessage.Define<string, string, string, string>(
                LogLevel.Information,
                new EventId(EventIds.Request, nameof(Request)),
                "{ClassName} -- {MethodName} -- request {RequestName}: {Parameters}.");

        private static readonly Action<ILogger, string, string, Exception?> s_requestWithoutParams = LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(EventIds.Request, nameof(Request)),
                "{ClassName} -- {MethodName} -- request.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_response = LoggerMessage.Define<string, string, string>(
                LogLevel.Information,
                new EventId(EventIds.Response, nameof(Response)),
                "{ClassName} -- {MethodName} -- response {Message}.");

        private static readonly Action<ILogger, string, string, Exception?> s_responseWithoutParams = LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(EventIds.Response, nameof(Response)),
                "{ClassName} -- {MethodName} -- response.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_errorResult = LoggerMessage.Define<string, string, string>(
                LogLevel.Error,
                new EventId(EventIds.ErrorResult, nameof(ErrorResult)),
                "{ClassName} -- {MethodName} -- response error result: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_errorMediatrResult = LoggerMessage.Define<string, string, string>(
                LogLevel.Error,
                new EventId(EventIds.ErrorMediatrResult, nameof(ErrorMediatrResult)),
                "{ClassName} -- {MethodName} -- response error Mediatr result: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_httpErrorResult = LoggerMessage.Define<string, string, string>(
                LogLevel.Error,
                new EventId(EventIds.HttpErrorResult, nameof(HttpErrorResult)),
                "{ClassName} -- {MethodName} -- response http error result: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_error = LoggerMessage.Define<string, string, string>(
                LogLevel.Error,
                new EventId(EventIds.Error, nameof(Error)),
                "{ClassName} -- {MethodName} -- error: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_errorException = LoggerMessage.Define<string, string, string>(
                LogLevel.Error,
                new EventId(EventIds.Error, nameof(Error)),
                "{ClassName} -- {MethodName} -- error: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_info = LoggerMessage.Define<string, string, string>(
                LogLevel.Information,
                new EventId(EventIds.Info, nameof(Info)),
                "{ClassName} -- {MethodName} -- info: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_warning = LoggerMessage.Define<string, string, string>(
                LogLevel.Warning,
                new EventId(EventIds.Warning, nameof(Warning)),
                "{ClassName} -- {MethodName} -- warning: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_debug = LoggerMessage.Define<string, string, string>(
                LogLevel.Debug,
                new EventId(EventIds.Debug, nameof(Debug)),
                "{ClassName} -- {MethodName} -- debug: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_job = LoggerMessage.Define<string, string, string>(
                LogLevel.Warning,
                new EventId(EventIds.Job, nameof(Job)),
                "------------QRTZ------------ {ClassName} -- {MethodName} -- Jobs: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_jobError = LoggerMessage.Define<string, string, string>(
                LogLevel.Error,
                new EventId(EventIds.JobError, nameof(JobError)),
                "------------QRTZ------------ {ClassName} -- {MethodName} -- Jobs: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_grpcError = LoggerMessage.Define<string, string, string>(
               LogLevel.Information,
               new EventId(EventIds.Grpc, nameof(Grpc)),
               "------------GRPC------------ {ClassName} -- {MethodName} -- message: {Message}.");

        private static readonly Action<ILogger, string, string, string, Exception?> s_grpc = LoggerMessage.Define<string, string, string>(
              LogLevel.Error,
              new EventId(EventIds.GrpcError, nameof(Job)),
              "------------GRPC------------ {ClassName} -- {MethodName} -- Error message: {Message}.");

        public static void Grpc(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_grpc(logger, className, methodName, message, e);
        }

        public static void GrpcError(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_grpcError(logger, className, methodName, message, e);
        }

        public static void Request(this ILogger logger, string className, string methodName, Exception? e = null)
        {
            s_requestWithoutParams(logger, className, methodName, e);
        }

        public static void Request(this ILogger logger, string className, string methodName, string requestName, object parameters, Exception? e = null)
        {
            s_request(logger, className, methodName, requestName, JsonSerializer.Serialize(parameters, s_jsonSerializerOption), e);
        }

        public static void Response(this ILogger logger, string className, string methodName, Exception? e = null)
        {
            s_responseWithoutParams(logger, className, methodName, e);
        }

        public static void Response(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_response(logger, className, methodName, message, e);
        }

        public static void ErrorResult(this ILogger logger, string className, string methodName, object message, Exception? e = null)
        {
            s_errorResult(logger, className, methodName, JsonSerializer.Serialize(message), e);
        }

        public static void ErrorMediatrResult(this ILogger logger, string className, string methodName, object message, Exception? e = null)
        {
            s_errorMediatrResult(logger, className, methodName, JsonSerializer.Serialize(message, s_jsonSerializerOption), e);
        }

        public static void HttpErrorResult(this ILogger logger, string className, string methodName, object message, Exception? e = null)
        {
            s_httpErrorResult(logger, className, methodName, JsonSerializer.Serialize(message, s_jsonSerializerOption), e);
        }

        public static void Error(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_error(logger, className, methodName, message, e);
        }

        public static void ErrorException(this ILogger logger, string className, string methodName, Exception? e = null)
        {
            s_errorException(logger, className, methodName, string.Format("Message: {0} -- StackTrace: {1}", e?.Message, e?.StackTrace), e);
        }

        public static void ErrorException(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_errorException(logger, className, methodName, string.Format("Message: {0} -- StackTrace: {1}", string.IsNullOrEmpty(message) ? e?.Message : message, e?.StackTrace), e);
        }

        public static void Info(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_info(logger, className, methodName, message, e);
        }

        public static void Warning(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_warning(logger, className, methodName, message, e);
        }

        public static void Debug(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_debug(logger, className, methodName, message, e);
        }

        public static void Job(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_job(logger, className, methodName, message, e);
        }

        public static void JobError(this ILogger logger, string className, string methodName, string message, Exception? e = null)
        {
            s_jobError(logger, className, methodName, message, e);
        }
    }
}
