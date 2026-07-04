using DAL.Entities;

namespace BLL.Interfaces
{
    public interface ISessionService
    {
        User? CurrentUser { get; set; }
        Shift? CurrentShift { get; set; }
        bool IsAuthenticated { get; }
    }
}
