using System.Net;
using System.Text.Json;
using eCommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.SharedLibrary.Middleware;

public class GlobalException(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
         // declare default variables   
         string message = "Sorry, Internal Servier Error Occured. Kindly try again";

         int statusCode = (int)HttpStatusCode.InternalServerError;

         string title = "Error";

         try
         {
             await next(context);
             
             // check if Response is too many requests //429 status code
             if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
             {
                 title = "Warning";
                 message = "Too Many requests made";
                 statusCode = StatusCodes.Status429TooManyRequests;
                 await ModifyHeader(context, title, message, statusCode);
             }
             
             // check if Response is UnAuthorized  //401 status code
             if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
             {
                 title = "Alert";
                 message = "You are not authorized to access";
                 await ModifyHeader(context, title, message, statusCode);
             }
             
             // if Response is Forbidden //403 status code
             if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
             {
                 title = "Out of Access";
                 message = "You are not allowed/required to access";
                 statusCode = StatusCodes.Status403Forbidden;
                 await ModifyHeader(context, title, message, statusCode);
             }
             
         }
         catch (Exception ex)
         {
             // Log Original exceptions / File, Debugger, Console
             LogException.LogExceptions(ex);
             
             // check if exception is timeout //408 request Timeout
             if (ex is TaskCanceledException || ex is TimeoutException)
             {
                 title = "Out of Time";
                 message = "Request timeout....Try Again .....";
                 statusCode = StatusCodes.Status408RequestTimeout;
             }
             // if exceptionis caught
             // if none of the exception, then do the default
             await ModifyHeader(context, title, message, statusCode);
         }

    }

    private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
    {
        // display scary-free message to client
        
        context.Response.ContentType = "Application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
        {
            Detail = message,
            Status = statusCode,
            Title = title
        }), CancellationToken.None);
        return;
    }
}