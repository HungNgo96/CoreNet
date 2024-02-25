// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Shared
{
    public class Result : IResult
    {
        public string? Message { get; set; }
        public bool Succeeded { get; set; }
        public int Code { get; set; }
    }

    public class Result<T> : IResult<T>
    {
        public T? Data { get; set; }

        public string? Message { get; set; }
        public bool Succeeded { get; set; }
        public int Code { get; set; }

        public static Result<T> Fail(int code = (int)HttpStatusCode.OK)
        {
            return new Result<T> { Succeeded = false, Code = code };
        }

        public static Result<T> Fail(string message, int code = (int)HttpStatusCode.OK)
        {
            return new Result<T> { Succeeded = false, Message = message, Code = code };
        }

        public static Result<T> Success(int code = (int)HttpStatusCode.OK)
        {
            return new Result<T> { Succeeded = true, Code = code };
        }

        public static Result<T> Success(string message, int code = (int)HttpStatusCode.OK)
        {
            return new Result<T> { Succeeded = true, Message = message, Code = code };
        }
    }
}
