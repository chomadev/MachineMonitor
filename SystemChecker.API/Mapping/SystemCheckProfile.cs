using AutoMapper;
using SystemChecker.API.Models;
using SystemChecker.API.Models.Dto;
using System.Text.Json;

namespace SystemChecker.API.Mapping;

public class SystemCheckProfile : Profile
{
    public SystemCheckProfile()
    {
        CreateMap<SystemCheckHistory, SystemCheckDto>();
        CreateMap<SystemCheckDto, SystemCheckHistory>();

        CreateMap<ServiceStatus, ServiceStatusDto>();
        CreateMap<ServiceStatusDto, ServiceStatus>();

        CreateMap<NetworkStatusDto, NetworkStatus>()
            .ForMember(dest => dest.ActiveInterfaces, opt => opt.MapFrom(src => 
                ConvertToJson(src.ActiveInterfaces)));

        CreateMap<MonitoredAddress, MonitoredAddressDto>();
        CreateMap<MonitoredAddressDto, MonitoredAddress>();

        CreateMap<DiskStatus, DiskStatusDto>();
        CreateMap<DiskStatusDto, DiskStatus>();

        CreateMap<CpuStatus, CpuStatusDto>();
        CreateMap<CpuStatusDto, CpuStatus>();

        CreateMap<MemoryStatus, MemoryStatusDto>();
        CreateMap<MemoryStatusDto, MemoryStatus>();

        CreateMap<TcpPortStatus, TcpPortStatusDto>();
        CreateMap<TcpPortStatusDto, TcpPortStatus>();

        CreateMap<FolderStatusDto, FolderStatus>()
            .ForMember(dest => dest.ZeroByteFiles, opt => opt.MapFrom(src => 
                ConvertToJson(src.ZeroByteFiles)));

        CreateMap<FolderChange, FolderChangeDto>();
        CreateMap<FolderChangeDto, FolderChange>();
    }

    private static string ConvertToJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj);
    }
} 