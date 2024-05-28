using MediatR;

namespace MUSbooking.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<TRequest> logger;

        public LoggingBehavior(ILogger<TRequest> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            using (logger.BeginScope(request))
            {
                logger.LogInformation("Calling handler...");
                var response = await next();
                logger.LogInformation($"Called handler with result {response}");
                return response;
            }
        }
    }
}
