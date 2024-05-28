using MediatR;
using MUSbooking.Database.Models;

namespace MUSbooking.Common.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly AppDbContext context;

        public TransactionBehavior(AppDbContext context) => this.context = context;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                await context.Database.BeginTransactionAsync();
                var response = await next();
                await context.Database.CommitTransactionAsync();
                return response;
            }
            catch (Exception)
            {
                context.Database.RollbackTransaction();
                throw;
            }
        }
    }
}
