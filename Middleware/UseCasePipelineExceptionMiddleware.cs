using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using UseCasePipeline.Exceptions;

namespace UseCasePipeline.Middleware
{
    /// <summary>
    /// Catches exceptions thrown by the UseCase pipeline and maps them to
    /// appropriate HTTP responses. Add to your app with
    /// <see cref="UseCasePipelineMiddlewareExtensions.UseUseCasePipelineExceptionHandler"/>.
    /// </summary>
    public class UseCasePipelineExceptionMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (UseCaseValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    ex.Message,
                    Errors = ex.Errors.Select(e => new { e.property, e.error })
                });
            }
            catch (UseCaseEntityNotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new
                {
                    ex.Message,
                    ex.EntityName,
                    ex.EntityId
                });
            }
            catch (UseCaseAuthorisationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    ex.Message
                });
            }
            catch (UseCaseRuleViolationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    ex.Message,
                    Errors = ex.Errors.Select(e => new { e.property, e.error })
                });
            }
        }
    }

    public static class UseCasePipelineMiddlewareExtensions
    {
        /// <summary>
        /// Adds the UseCase pipeline exception handler to the middleware pipeline.
        /// Maps <see cref="UseCaseValidationException"/> → 400,
        /// <see cref="UseCaseEntityNotFoundException"/> → 404, and
        /// <see cref="UseCaseAuthorisationException"/> → 403, and
        /// <see cref="UseCaseRuleViolationException"/> → 400.
        /// Register this before any other middleware that invokes use cases.
        /// </summary>
        public static IApplicationBuilder UseUseCasePipelineExceptionHandler(
            this IApplicationBuilder app)
            => app.UseMiddleware<UseCasePipelineExceptionMiddleware>();
    }
}
