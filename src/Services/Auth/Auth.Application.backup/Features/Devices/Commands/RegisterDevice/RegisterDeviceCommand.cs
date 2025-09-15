using BuildingBlocks.Common.Models;
using MediatR;

namespace Auth.Application.Features.Devices.Commands.RegisterDevice;

public class RegisterDeviceCommand : IRequest<ApiResponse<RegisterDeviceResponse>>
{
    public Guid UserId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class RegisterDeviceResponse
{
    public Guid DeviceId { get; set; }
    public string DeviceKey { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}