using AutoMapper;
using MapApp.API.Models;
using MapApp.API.DTOs;

namespace MapApp.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // StateCapital mapping
        CreateMap<StateCapital, StateCapitalDto>()
            .ForMember(dest => dest.PinColor, opt => opt.Ignore()); // Set in controller

        CreateMap<StateCapitalDto, StateCapital>();

        // Route mapping
        CreateMap<RouteSegment, RouteSegmentDto>();
        CreateMap<RouteSegmentDto, RouteSegment>();

        CreateMap<OptimizedRoute, OptimizedRouteDto>()
            .ForMember(dest => dest.StateNames, opt => opt.Ignore()); // Set in controller

        CreateMap<OptimizedRouteDto, OptimizedRoute>();

        // Transportation plan mapping
        CreateMap<TransportationPlan, TransportationPlanDto>();
        CreateMap<TransportationPlanDto, TransportationPlan>();
    }
}
