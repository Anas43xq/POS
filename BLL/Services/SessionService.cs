using BLL.Interfaces;
using DAL.Entities;

namespace BLL.Services
{
    public class SessionService : ISessionService
    {
        public User? CurrentUser { get; set; }
        public Shift? CurrentShift { get; set; }

        public bool IsAuthenticated => CurrentUser != null;
    }
}
