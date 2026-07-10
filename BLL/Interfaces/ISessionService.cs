using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ISessionService
    {
        UserDto? CurrentUser { get; set; }
        ShiftDto? CurrentShift { get; set; }
        bool IsAuthenticated { get; }
    }
}