// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PadTime.API.Attributes;

/// <summary>
/// Action filter attribute that validates request models using FluentValidation.
/// Provides automatic validation for API request DTOs with consistent error responses.
/// </summary>
public sealed class ValidateModelAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Executes FluentValidation for each action argument before the action runs.
    /// Returns a 400 Bad Request with <see cref="ValidationProblemDetails"/> if validation fails.
    /// </summary>
    /// <param name="context">The action executing context containing the action arguments.</param>
    /// <param name="next">The delegate to invoke the next filter or action in the pipeline.</param>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;
            
            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            
            if (serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);
                
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray(),
                            StringComparer.Ordinal);

                    var problemDetails = new ValidationProblemDetails(errors)
                    {
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        Title = "One or more validation errors occurred",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Please check the errors property for details."
                    };

                    context.Result = new BadRequestObjectResult(problemDetails);
                    return;
                }
            }
        }
        
        await next();
    }
}