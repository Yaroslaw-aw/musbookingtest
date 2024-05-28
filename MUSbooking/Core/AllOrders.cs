using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MUSbooking.Common;
using MUSbooking.Common.Caching;
using MUSbooking.Common.Mapping;
using MUSbooking.Database.Models;
using MUSbooking.Database.Models.Connections;
using static MUSbooking.Core.AllOrders;

namespace MUSbooking.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class AllOrdersController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<OrderDto>>> ShowAllOrders(int page = 1, int pageSize = 30)
            => HandleResult(await Mediator.Send(new Query(page, pageSize)));
    }
    public class AllOrders
    {
        public record Query(int page, int pageSize) : IRequest<ValidationResult<PaginatedResult<OrderDto>>>;

        public record Handler(AppDbContext context, IMapper mapper, Redis redis) : IRequestHandler<Query, ValidationResult<PaginatedResult<OrderDto>>>
        {
            public async Task<ValidationResult<PaginatedResult<OrderDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                //string cacheKey = $"Orders_{request.page}_{request.pageSize}";

                //var (success, cachedResult) = await redis.TryGetValueAsync<PaginatedResult<OrderDto>>(cacheKey);
                //if (success)
                //{
                //    return ValidationResult<PaginatedResult<OrderDto>>.Success(cachedResult);
                //}

                //// Данные отсутствуют в кэше, выполняем запрос к базе данных
                //IOrderedQueryable<Order> ordersQuery = context.Orders
                //    .AsNoTracking()
                //    .Include(o => o.OrderEquipments)
                //    .ThenInclude(oe => oe.Equipment)
                //    .OrderBy(o => o.CreatedAt);

                //int totalItems = await ordersQuery.CountAsync(cancellationToken);
                //List<Order> orders = await ordersQuery
                //    .Skip((request.page - 1) * request.pageSize)
                //    .Take(request.pageSize)
                //    .ToListAsync(cancellationToken);

                //if (orders.Any())
                //{
                //    List<OrderDto> orderDtos = mapper.Map<List<OrderDto>>(orders);
                //    PaginatedResult<OrderDto> paginatedResult = new PaginatedResult<OrderDto>(orderDtos, totalItems, request.page, request.pageSize);

                //    // Кэшируем результат запроса на указанное время (например, 5 минут)
                //    await redis.SetDataAsync(cacheKey, paginatedResult, TimeSpan.FromMinutes(5).Seconds);

                //    return ValidationResult<PaginatedResult<OrderDto>>.Success(paginatedResult);
                //}
                //else
                //{
                //    return ValidationResult<PaginatedResult<OrderDto>>.Failure("There are no orders in the system");
                //}


                IOrderedQueryable<Order>? ordersQuery = context.Orders
                .AsNoTracking()
                .Include(o => o.OrderEquipments)
                .ThenInclude(oe => oe.Equipment)
                .OrderBy(o => o.CreatedAt);

                int totalItems = await ordersQuery.CountAsync(cancellationToken);
                List<Order> orders = await ordersQuery
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .ToListAsync(cancellationToken);

                if (orders.Any())
                {
                    List<OrderDto> orderDtos = mapper.Map<List<OrderDto>>(orders);
                    PaginatedResult<OrderDto> paginatedResult = new PaginatedResult<OrderDto>(orderDtos, totalItems, request.page, request.pageSize);
                    return ValidationResult<PaginatedResult<OrderDto>>.Success(paginatedResult);
                }
                else
                {
                    return ValidationResult<PaginatedResult<OrderDto>>.Failure("There are no orders in the system");
                }
            }
        }
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; }
        public int TotalItems { get; }
        public int Page { get; }
        public int PageSize { get; }

        public PaginatedResult(List<T> items, int totalItems, int page, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            Page = page;
            PageSize = pageSize;
        }
    }

    public class OrderDto : IMapWith<Order>
    {
        public Guid OrderId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal Price { get; set; }
        public List<OrderEquipmentDto> Equipments { get; set; } = new List<OrderEquipmentDto>();

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Order, OrderDto>()
                .ForMember(orderDto => orderDto.Equipments, opt => opt.MapFrom(order => order.OrderEquipments));
        }
    }

    public class OrderEquipmentDto : IMapWith<OrderEquipments>
    {
        public Guid EquipmentId { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OrderEquipments, OrderEquipmentDto>()
            .ForMember(orderEquipmentDto => orderEquipmentDto.EquipmentId, opt => opt.MapFrom(orderEquipment => orderEquipment.Equipment.EquipmentId))
            .ForMember(orderEquipmentDto => orderEquipmentDto.Name, opt => opt.MapFrom(orderEquipment => orderEquipment.Equipment.Name))
            .ForMember(orderEquipmentDto => orderEquipmentDto.Price, opt => opt.MapFrom(orderEquipment => orderEquipment.Equipment.Price))
            .ForMember(orderEquipmentDto => orderEquipmentDto.Quantity, opt => opt.MapFrom(orderEquipment => orderEquipment.Quantity));
        }
    }
}