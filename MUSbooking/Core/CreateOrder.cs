using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MUSbooking.Common;
using MUSbooking.Common.Exceptions;
using MUSbooking.Database.Models;
using MUSbooking.Database.Models.Connections;

namespace MUSbooking.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreateOrderController : BaseController
    {
        [HttpPost]
        public async Task<ActionResult<Guid?>> CreateOrder([FromBody] CreateOrderDto requestDto)
            => HandleResult(await Mediator.Send(new CreateOrder.Command(requestDto)));
    }

    public class CreateOrder
    {
        public class Command : IRequest<ValidationResult<Guid?>>
        {
            public CreateOrderDto RequestDto { get; set; }
            public Command(CreateOrderDto requestDto)
            {
                RequestDto = requestDto;
            }
        }

        public record Handler(CreateOrderService service) : IRequestHandler<Command, ValidationResult<Guid?>>
        {
            public async Task<ValidationResult<Guid?>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    Guid? orderId = await service.CreateOrder(request.RequestDto, cancellationToken);

                    return ValidationResult<Guid?>.Success(orderId);
                }
                catch (Exception exception)
                {
                    return ValidationResult<Guid?>.Failure(exception.Message);
                }
            }
        }

        public class Validator : AbstractValidator<Order>
        {
            public Validator()
            {

            }
        }
    }

    public class CreateOrderService(AppDbContext context)
    {
        public async Task<Guid?> CreateOrder(CreateOrderDto orderDto, CancellationToken cancellationToken)
        {
            if (orderDto.Equipments is null || orderDto.Equipments.Count == 0)
            {
                throw new ArgumentException("Equipment list cannot be null or empty.", nameof(orderDto.Equipments));
            }

            Order newOrder = new Order();

            if (string.IsNullOrEmpty(orderDto.Description) is false)
                newOrder.Description = orderDto.Description;

            try
            {
                await context.Orders.AddAsync(newOrder, cancellationToken);

                foreach (var equipmentOrder in orderDto.Equipments)
                {
                    Equipment? equipment = await context.Equipments.FirstOrDefaultAsync(eq => eq.EquipmentId == equipmentOrder.EquipmentId, cancellationToken);

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
                        OrderId = newOrder.OrderId,
                        EquipmentId = equipment.EquipmentId,
                        Quantity = equipmentOrder.Quantity
                    });
                    
                    equipment.Amount -= equipmentOrder.Quantity;
                    context.Equipments.Update(equipment);
                    
                    newOrder.Price += equipment.Price * equipmentOrder.Quantity;
                }

                await context.SaveChangesAsync(cancellationToken);

                return newOrder.OrderId;
            }
            catch (Exception)
            {
                throw;
            }
        }        
    }

    public class CreateOrderDto
    {
        public List<EquipmentOrderDto> Equipments { get; set; } = new List<EquipmentOrderDto>();
        public string? Description { get; set; }
    }

    public class EquipmentOrderDto
    {
        public Guid EquipmentId { get; set; }
        public int Quantity { get; set; }
    }
}