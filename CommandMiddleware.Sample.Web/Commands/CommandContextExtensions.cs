using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CommandMiddleware.Sample.Web.Commands
{
    public static class CommandContextExtensions
    {
        public static Task<ActionResult> ToActionResult(this Task<CommandContext> command,
            Func<object, ActionResult> onSuccess = null,
            Func<IEnumerable<DomainError>, ActionResult> onDomainError = null)
        {
            return command.ToActionResult<object>(onSuccess, onDomainError);
        }
        
        public static async Task<ActionResult> ToActionResult<TResponse>(this Task<CommandContext> command,
            Func<TResponse, ActionResult> onSuccess = null,
            Func<IEnumerable<DomainError>, ActionResult> onDomainError = null)
        {
            var context = await command;

            if (onSuccess == null)
            {
                onSuccess = OkOrNoContent;
            }
            
            if (onDomainError == null)
            {
                onDomainError = BadRequest;
            }

            switch (context.Response)
            {
                case TResponse response:
                    return onSuccess(response);
                
                case DomainError error:
                    return onDomainError(new[] {error});
                
                case IEnumerable<DomainError> errors:
                    return onDomainError(errors);
                
                default:
                    throw new InvalidOperationException("Unexpected command response. This is probably developer error, ensure the command output can be handled.");
            }
        }

        private static ActionResult OkOrNoContent<TResponse>(TResponse response)
        {
            if (response != null)
            {
                return new OkObjectResult(response);
            }
            
            return new NoContentResult();
        }

        private static ActionResult BadRequest(IEnumerable<DomainError> errors)
        {
            var modelState = new ModelStateDictionary();
            foreach (var error in errors)
            {
                modelState.AddModelError(error.Key, error.Message);
            }
            
            return new BadRequestObjectResult(modelState);
        }
    }
}