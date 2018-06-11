namespace Alpha.LogCollector.WebService.Filters
{
    using System.Web.Http.Filters;
    using Fanex.Logging.Extensions.Web;

    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);

            ExceptionHandler.Handle(
                                actionExecutedContext.Exception
                                // To Write Custom Data
                                // new Dictionary<string, object> { ["user id"] = 1111111 }
                                );
        }
    }
}