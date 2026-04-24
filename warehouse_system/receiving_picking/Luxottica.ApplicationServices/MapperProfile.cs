using AutoMapper;
using Luxottica.ApplicationServices.Shared.Dto.CameraAssignments;
using Luxottica.ApplicationServices.Shared.Dto.DivertLines;
using Luxottica.Core.Entities.CameraAssignments;
using Luxottica.Core.Entities.DivertLines;
using Luxottica.ApplicationServices.Shared.Dto.Users;
using Luxottica.Core.Entities.PhysicalMaps;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luxottica.Core.Entities.JackpotLines;
using Luxottica.ApplicationServices.Shared.Dto.JackpotLines;
using Luxottica.ApplicationServices.Shared.Dto.MapPhysicVirtualSAP;
using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.Core.Entities.Cameras;
using Luxottica.Core.Entities.ToteHdrs;
using Luxottica.ApplicationServices.Shared.Dto.PickingJackpotLines;
using Luxottica.Core.Entities.CameraAssignments.SecondLevel;
using Luxottica.ApplicationServices.Shared.Dto.SecondLevelCamera;
using Luxottica.Core.Entities.LimitsSettings;
using Luxottica.ApplicationServices.Shared.Dto.LimitSettings;
using Luxottica.Core.Entities.HighwayPikingLanes;
using Luxottica.ApplicationServices.Shared.Dto.HighwayPickingLanes;
using Luxottica.Core.Entities.DivertOutboundLines;
using Luxottica.ApplicationServices.Shared.Dto.DivertOutboundLines;
using Luxottica.Core.Entities.Acknowledgments;
using Luxottica.ApplicationServices.Shared.Dto.Acknowledgment;
using Luxottica.Core.Entities.Scanlogs;
using Luxottica.ApplicationServices.Shared.Dto.Scanlogs;

namespace Luxottica.ApplicationServices
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<MapPhysicalVirtualSAP, MapPhysicVirtualSAPDto>();
            CreateMap<MapPhysicVirtualSAPAddDto, MapPhysicalVirtualSAP>()
            .ForPath(dest => dest.DivertLine.Id, opt => opt.MapFrom(src => src.DivertLineId));

            CreateMap<CameraAssignment, CameraAssignmentDto>();
            CreateMap<CameraAssignmentAddDto, CameraAssignment>()
            .ForPath(dest => dest.DivertLine.Id, opt => opt.MapFrom(src => src.DivertLineId))
            .ForPath(dest => dest.Camera.Id, opt => opt.MapFrom(src => src.CameraId));

            CreateMap<JackpotLine, JackpotLineDto>();
            CreateMap<JackpotLineAddDto, JackpotLine>()
                .ForPath(dest => dest.DivertLine.Id, opt => opt.MapFrom(src => src.DivertLineId));

            CreateMap<PickingJackpotLine, PickingJackpotLineDto>();
            CreateMap<PickingJackpotLine, PickingJackpotLineGetDto>();
            CreateMap<PickingJackpotLineAddDto, PickingJackpotLine>()
                .ForPath(dest => dest.DivertLine.Id, opt => opt.MapFrom(src => src.DivertLineId));

            CreateMap<SecondLevelCamera, SecondLevelCameraGetDto>();
            CreateMap<SecondLevelCameraAddDto, SecondLevelCamera>()
                .ForPath(dest => dest.Camera.Id, opt => opt.MapFrom(src => src.CameraId));

            CreateMap<DivertLine, DivertLineDto>();
            CreateMap<DivertLineAddDto, DivertLine>();

            CreateMap<IdentityUser, UserDto>();
            CreateMap<IdentityUser, NewUserDto>();
            CreateMap<IdentityUser, EditUserDto>();
            CreateMap<UserDto, IdentityUser>();
            CreateMap<NewUserDto, IdentityUser>();
            CreateMap<EditUserDto, IdentityUser>();
            CreateMap<IdentityRole, RolesNameDto>();

            CreateMap<CameraDTO, Camera>();
            CreateMap<LimitSetting, LimitSettingDTO>();
            CreateMap<LimitSettingDTO, LimitSetting>()
            .ForPath(dest => dest.Camera.Id, opt => opt.MapFrom(src => src.CameraId));
            CreateMap<Camera, CameraDTO>();


            CreateMap<HighwayPikingLane, HighwayPickingLanesDTO>();
            CreateMap<HighwayPickingLanesDTO, HighwayPikingLane>()
            .ForPath(dest => dest.Commissioner.Id, opt => opt.MapFrom(src => src.CommissionerId));

            CreateMap<DivertOutboundLine, DivertOutboundLineDTO>();
            CreateMap<DivertOutboundLineDTO, DivertOutboundLine>()
            .ForPath(dest => dest.Commissioner.Id, opt => opt.MapFrom(src => src.CommissionerId));

            CreateMap<Acknowledgment, AcknowledgmentDTO>();
            CreateMap<AcknowledgmentDTO, Acknowledgment>();

            CreateMap<Acknowledgment, AcknowledgmentAddDTO>();
            CreateMap<AcknowledgmentAddDTO, Acknowledgment>();

            CreateMap<ScanlogsReceivingPicking, ScanlogsGetDto>();
            CreateMap<ScanlogsReceivingPicking, ScanlogsAddDto>();
            CreateMap<ScanlogsAddDto, ScanlogsReceivingPicking>();
        }
    }
}
