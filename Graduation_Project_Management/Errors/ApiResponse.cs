﻿
namespace Graduation_Project_Management.Errors
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }

        public ApiResponse(int statusCode , string? message=null )
        {
            StatusCode = statusCode;
            Message = message??GetDefaultMessageForStatusCode(StatusCode);
            
        }

        private string? GetDefaultMessageForStatusCode(int? statusCode)
        {
            return StatusCode switch
            {
                400 => "Bad Request",
                401 => "You Are not Authorized",
                404 => "Resource Not Found",
                500 => "Internal Server Error,",
                _ => null
            };
        }
    }
}
