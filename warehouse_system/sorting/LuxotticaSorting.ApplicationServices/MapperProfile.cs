using AutoMapper;
using LuxotticaSorting.ApplicationServices.Shared.Dto.ContainerTypes;
using LuxotticaSorting.ApplicationServices.Shared.Dto.LogisticAgents;
using LuxotticaSorting.ApplicationServices.Shared.Dto.RoutingV10S;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.BoxTypes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeDivertLaneMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodeLogisticAgent;
using LuxotticaSorting.ApplicationServices.Shared.DTO.CarrierCodes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLanes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.DivertLaneZebraConfigurationMapping;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MappingSorters;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RecirculationLimits;
using LuxotticaSorting.ApplicationServices.Shared.DTO.MultiBoxWaves;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Zebra;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ZebraHistorial;
using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.ContainerTypes;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.LogisticAgents;
using LuxotticaSorting.Core.Mapping.BoxTypeDivertLane;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.Core.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.Core.Mapping.DivertLaneZebraConfiguration;
using LuxotticaSorting.Core.RecirculationLimits;
using LuxotticaSorting.Core.MultiBoxWaves;
using LuxotticaSorting.Core.WCSRoutingV10;
using LuxotticaSorting.Core.Zebra;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;

namespace LuxotticaSorting.ApplicationServices
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CarrierCode, CarrierCodeAddDto>();
            CreateMap<CarrierCodeAddDto, CarrierCode>();

            CreateMap<BoxType, BoxTypesAddDTO>();
            CreateMap<BoxTypesAddDTO, BoxType>();

            CreateMap<WCSRoutingV10, RoutingV10Dto>();
            CreateMap<RoutingV10Dto, WCSRoutingV10>();
            //CreateMap<BoxAddDto, Box>();

            CreateMap<LogisticAgent, LogisticAgentDto>();
            CreateMap<LogisticAgentAddDto, LogisticAgent>();

            CreateMap<ContainerType, ContainerTypeDto>();
            CreateMap<ContainerTypeAddDto, ContainerType>();
            CreateMap<DivertLane, DivertLanesDTO>();
            CreateMap<DivertLane, DivertLanesCreationDTO>();
            CreateMap<DivertLanesAddDto, DivertLane>();

            CreateMap<CarrierCodeDivertLaneMapping, CarrierCodeDivertLaneMappingDto>();
            CreateMap<CarrierCodeDivertLaneMappingAddDto, CarrierCodeDivertLaneMapping>();
            CreateMap<CarrierCodeDivertLaneMappingEditDto, CarrierCodeDivertLaneMapping>();

            CreateMap<BoxTypeDivertLaneMapping, BoxTypeDivertLaneMappingAddDto>();
            CreateMap<BoxTypeDivertLaneMappingAddDto, BoxTypeDivertLaneMapping>();

            CreateMap<CarrierCodeLogisticAgentMapping, CarrierCodeLogisticAgentDto>();
            CreateMap<CarrierCodeLogisticAgentAddDto, CarrierCodeLogisticAgentMapping>();

            CreateMap<Core.Zebra.ZebraHistorial, ZebraHistorialDTO>();


            CreateMap<DivertLaneZebraConfigurationMapping, DivertLaneZebraConfigurationMappingDTO>();
            CreateMap<DivertLaneZebraConfigurationMappingAddDTO, DivertLaneZebraConfigurationMapping>();

            CreateMap<ZebraConfiguration, ZebraConfigurationDTO>();
            CreateMap<ZebraConfigurationAddDTO, ZebraConfiguration>();

            CreateMap<Core.MappingSorter.MappingSorter, MappingSorterDto>();
            CreateMap<MappingSorterAddDto, Core.MappingSorter.MappingSorter>();

            CreateMap<MultiBoxWave, MultiBoxWavesAddDto>();
            CreateMap<MultiBoxWavesAddDto, MultiBoxWave>();

            CreateMap<MultiBoxWave, MultiBoxWavesGetDto>();
            CreateMap<MultiBoxWavesGetDto, MultiBoxWave>();

            CreateMap<ContainerTable, ContainerDTO>();
            CreateMap<ContainerAddDTO, ContainerTable>()
            .ForPath(dest => dest.ContainerType.Id, opt => opt.MapFrom(src => src.ContainerTypeId));
            CreateMap<ContainerAddOneStepDTO, ContainerTable>()
            .ForPath(dest => dest.ContainerType.Id, opt => opt.MapFrom(src => src.ContainerTypeId));

            CreateMap<RecirculationLimit, RecirculationLimitDto>();
            CreateMap<RecirculationLimitAddDto, RecirculationLimit>();

        }
    }
}
