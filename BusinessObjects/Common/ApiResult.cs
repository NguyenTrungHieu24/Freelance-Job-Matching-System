using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Common
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public List<string>? Errors { get; set; }

        public static ApiResult<T> Ok(
        T? data = default,
        string message = "")
        {
            return new()
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResult<T> Fail(
            string message,
            params string[] errors)
        {
            return new()
            {
                Success = false,
                Message = message,
                Errors = errors.ToList()
            };
        }
    }
}
