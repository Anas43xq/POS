using BLL.DTOs;
using BLL.Interfaces;

namespace BLL.Services
{
    public class SessionService : ISessionService
    {
        public UserDto? CurrentUser { get; set; }
        public ShiftDto? CurrentShift { get; set; }

        public bool IsAuthenticated => CurrentUser != null;
    }
}