using CheckinLabs.BL.Models;
using System.Threading.Tasks;

namespace CheckinLabs.BL.Svc
{
    public interface INotifier<T> where T : IdObject
    {
        Task CreateNotificationAsync(T subj);
    }
}
