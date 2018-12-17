using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CommandMiddleware.Sample.Web.Commands
{
    public class CommandActionResultBuilder
    {
        private readonly Dictionary<Type, Func<object, ActionResult>> _handlers = new Dictionary<Type, Func<object, ActionResult>>();
        private readonly Task<CommandResult> _command;

        internal CommandActionResultBuilder(Task<CommandResult> command)
        {
            _command = command;

            Match<DomainError>(error => BadRequest(new[] {error}));
            Match<IEnumerable<DomainError>>(BadRequest);
        }

        public CommandActionResultBuilder Match<T>(Func<T, ActionResult> handler)
        {
            _handlers[typeof(T)] = x => handler((T)x);
            return this;
        }

        public async Task<ActionResult> ToActionResult()
        {
            var result = await _command;

            if (_handlers.TryGetValue(result.State.GetType(), out var handler))
            {
                return handler(result.State);
            }
            
            if (result.State.Equals(Command.NoResponse))
            {
                return new NoContentResult();
            }
            
            return new OkObjectResult(result.State);
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