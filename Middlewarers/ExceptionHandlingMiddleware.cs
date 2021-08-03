using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using webapi_identity.Exceptions;
using webapi_identity.Domains;


namespace webapi_identity.Middlewarers
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (UserAlreadyExistsException ex)
            {
                ExceptionDetails exceptiondetails = new ExceptionDetails
                {

                    Message = ex.Message
                };
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await context.Response.WriteAsJsonAsync(exceptiondetails);
            }
            catch (UserNotFoundException ex)
            {
                ExceptionDetails exceptiondetails = new ExceptionDetails
                {

                    Message = ex.Message
                };
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsJsonAsync(exceptiondetails);
            }
            catch (InvalidUserException ex)
            {
                ExceptionDetails exceptiondetails = new ExceptionDetails
                {

                    Message = ex.Message
                };
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsJsonAsync(exceptiondetails);
            }
            catch (Exception ex)
            {

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                ExceptionDetails exceptiondetails = new ExceptionDetails
                {

                    Message = ex.Message
                };
                await context.Response.WriteAsJsonAsync(exceptiondetails);
            }
        }
    }
}