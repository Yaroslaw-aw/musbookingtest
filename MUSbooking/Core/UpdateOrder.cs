using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MUSbooking.Common;
using MUSbooking.Common.Exceptions;
using MUSbooking.Database.Models;
using MUSbooking.Database.Models.Connections;
using static MUSbooking.Core.UpdateOrder;
using Order = MUSbooking.Database.Models.Order;

namespace MUSbooking.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpdateOrderController : BaseController
    {
        [HttpPut]
        public async Task<ActionResult<Guid?>> UpdateOrder([FromQuery] Guid orderId, [FromBody] UpdateOrderDto? updateOrderDto)
            => await Mediator.Send(new Command(orderId, updateOrderDto));
    }

    public class UpdateOrder
    {
        public record Command(Guid orderId, UpdateOrderDto? updateOrderDto) : IRequest<Guid?>;

        public record Handler(UpdateOrderService service) : IRequestHandler<Command, Guid?>
        {
            public async Task<Guid?> Handle(Command request, CancellationToken cancellationToken)
            {
                if (request.updateOrderDto is null) return null;

                return await service.UpdateOrderAsync(request.orderId, request.updateOrderDto);
            }
        }
    }

    public class UpdateOrderService(AppDbContext context)
    {
        public async Task<Guid?> UpdateOrderAsync(Guid orderId, UpdateOrderDto updateOrderDto)
        {
            Order? existingOrder = await context.Orders
            .Include(o => o.OrderEquipments)
            .ThenInclude(oe => oe.Equipment)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (existingOrder is null)
            {
                throw new NotFoundException(nameof(Order), orderId);
            }

            if(string.IsNullOrEmpty(updateOrderDto.Description) is false)
                existingOrder.Description = updateOrderDto.Description;

            // Обновление позиций заказа
            foreach (var equipmentOrder in updateOrderDto.Equipments)
            {
                OrderEquipments? existingOrderEquipment = existingOrder.OrderEquipments
                    .FirstOrDefault(oe => oe.EquipmentId == equipmentOrder.EquipmentId);

                if (existingOrderEquipment is not null)
                {
                    // Позиция заказа уже существует, обновляем количество
                    existingOrderEquipment.Quantity = equipmentOrder.Quantity;
                }
                else
                {
                    // Позиция заказа не существует, добавляем новую
                    Equipment? equipment = await context.Equipments.FindAsync(equipmentOrder.EquipmentId);

                    if (equipment is null)
                    {
                        throw new NotFoundException(nameof(Equipment), equipmentOrder.EquipmentId);
                    }

                    if (equipment.Amount < equipmentOrder.Quantity)
                    {
                        throw new Exception($"Not enough stock for equipment {equipment.Name}. Requested: {equipmentOrder.Quantity}, Available: {equipment.Amount}");
                    }

                    context.OrderEquipments.Add(new OrderEquipments
                    {
                        OrderId = orderId,
                        EquipmentId = equipment.EquipmentId,
                        Quantity = equipmentOrder.Quantity
                    });

                    // Обновление количества оборудования на складе
                    equipment.Amount -= equipmentOrder.Quantity;
                    context.Equipments.Update(equipment);
                }
            }

            // Удаление позиций заказа, которых уже нет в списке обновления
            foreach (var existingOrderEquipment in existingOrder.OrderEquipments.ToList())
            {
                EquipmentUpdateOrderDto? updatedOrderEquipment = updateOrderDto.Equipments
                    .FirstOrDefault(oe => oe.EquipmentId == existingOrderEquipment.EquipmentId);
                if (updatedOrderEquipment == null)
                {
                    // Возвращаем количество удаленного оборудования на склад
                    var equipment = await context.Equipments.FindAsync(existingOrderEquipment.EquipmentId);
                    if (equipment != null)
                    {
                        equipment.Amount += existingOrderEquipment.Quantity;
                        context.Equipments.Update(equipment);
                    }

                    // Удаляем позицию заказа
                    context.OrderEquipments.Remove(existingOrderEquipment);
                }
            }

            await context.SaveChangesAsync();

            return existingOrder.OrderId;
        }        
    }


    public class UpdateOrderDto
    {
        public string? Description { get; set; }
        public List<EquipmentUpdateOrderDto> Equipments { get; set; } = new List<EquipmentUpdateOrderDto>();
    }

    public class EquipmentUpdateOrderDto
    {
        public Guid EquipmentId { get; set; }
        public int Quantity { get; set; }
    }

}
