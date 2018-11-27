//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//
//namespace CommandMiddleware.Sample.Web.Commands
//{
//    public delegate Task<ActionResult> HttpCommandDelegate(object command, HttpContext httpContext);
//
//    public static class CommandDelegateExtensions
//    {
//        public static HttpCommandDelegate AsHttpCommandDelegate(this CommandDelegate commandProcessor)
//        {
//            return async (command, httpContext) =>
//            {
//                await commandProcessor(command);
//                return new CreatedResult("hello world", null);
//            };
//        }
//    }
//}