using System;

namespace Core
{
    public class Result
    {
        public string ErrorMessage { get; private set; }
        public bool IsSuccess { get; private set; }

        public static Result CreateSuccess()
        {
            return new Result
            {
                IsSuccess = true
            };
        }

        public static Result CreateFail(string errorMessage)
        {
            return new Result
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}