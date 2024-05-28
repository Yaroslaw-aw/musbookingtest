using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MUSbooking.Common;
using MUSbooking.Common.Exceptions;
using MUSbooking.Database.Models;
using static MUSbooking.Core.DeleteOrder;

namespace MUSbooking.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeleteOrderController : BaseController
    {
        [HttpDelete]
        public async Task<ActionResult<Guid?>> DeleteOrder(Guid orderId)
            => await Mediator.Send(new Command(orderId));
    }

    public class DeleteOrder
    {
        public record Command(Guid orderId) : IRequest<Guid?>;

        public record Handler(AppDbContext context) : IRequestHandler<Command, Guid?>
        {
            public async Task<Guid?> Handle(Command request, CancellationToken cancellationToken)
            {
                Order? orderToDelete = await context.Orders
                    .Include(o => o.Equipments)
                    .FirstOrDefaultAsync(order => order.OrderId == request.orderId);

                if (orderToDelete is null) throw new NotFoundException(nameof(Order), request.orderId);

                Equipment? equipment;

                foreach (var orderEquipment in orderToDelete.OrderEquipments)
                {
                    equipment = await context.Equipments.FindAsync(orderEquipment.EquipmentId);

                    if (equipment is not null)
                    {
                        equipment.Amount += orderEquipment.Quantity;
                        context.Equipments.Update(equipment);
                    }
                }

                context.Orders.Remove(orderToDelete);

                await context.SaveChangesAsync(cancellationToken);

                return orderToDelete.OrderId;
            }
        }
    }
}