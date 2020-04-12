using System;

namespace GenericMiddlewarePipeline
{
    internal class InvalidMiddlewareException : Exception
    {
        public InvalidMiddlewareException(string message) : base(message)
        {

        }
    }
}
