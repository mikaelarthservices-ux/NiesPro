using BuildingBlocks.Common.Models;
using MediatR;

namespace Auth.Application.Features.Devices.Queries.GetUserDevices;

public class GetUserDevicesQuery : IRequest<ApiResponse<List<UserDeviceDto>>>
{
    public Guid UserId { get; set; }
}

public class UserDeviceDto
{
    public Guid Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? LastIpAddress { get; set; }
}