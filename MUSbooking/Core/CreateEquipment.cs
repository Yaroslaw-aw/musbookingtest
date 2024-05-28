using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MUSbooking.Common;
using MUSbooking.Common.Mapping;
using MUSbooking.Database.Models;
using static MUSbooking.Core.CreateEquipment;

namespace MUSbooking.Core
{

    [ApiController]
    [Route("api/[controller]")]
    public class CreateEquipmentController : BaseController
    {
        /// <summary>
        /// Create new equipment
        /// </summary>
        /// <param name="equipmentDto"></param>
        /// <returns>id of new created equipment</returns>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateEquipment([FromBody] CreateEquipmentDto requestDto)
            => HandleResult(await Mediator.Send(new Command(requestDto)));
    }

    public class CreateEquipment
    {
        public class Command : IRequest<ValidationResult<Guid>>
        {
            public CreateEquipmentDto? RequestDto { get; set; }
            public Command(CreateEquipmentDto requestDto)
            {
                RequestDto = requestDto;
            }
        }

        public record Handler(IMapper mapper, AppDbContext context) : IRequestHandler<Command, ValidationResult<Guid>>
        {
            public async Task<ValidationResult<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                Equipment newEquipment = mapper.Map<Equipment>(request.RequestDto);

                await context.AddAsync(newEquipment, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return ValidationResult<Guid>.Success(newEquipment.EquipmentId);
            }
        }

        public class Validator : AbstractValidator<Equipment>
        {
            public Validator()
            {
                RuleFor(e => e.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must be less than 100 characters");

                RuleFor(e => e.Amount)
                    .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be less than 0");

                RuleFor(e => e.Price)
                    .GreaterThanOrEqualTo(0).WithMessage("Price cannot be less than 0");
            }
        }        
    }

    public class CreateEquipmentDto : IMapWith<Equipment>
    {
        public string? name { get; set; }
        public int? amount { get; set; }
        public decimal price { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateEquipmentDto, Equipment>()
                .ForMember(newEquipment => newEquipment.Name, opts => opts.MapFrom(requestDto => requestDto.name))
                .ForMember(newEquipment => newEquipment.Amount, opts => opts.MapFrom(requestDto => requestDto.amount))
                .ForMember(newEquipment => newEquipment.Price, opts => opts.MapFrom(requestDto => requestDto.price))
                .ReverseMap();
        }
    }
}