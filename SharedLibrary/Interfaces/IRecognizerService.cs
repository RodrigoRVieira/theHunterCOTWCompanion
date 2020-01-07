using SharedLibrary.Models;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IRecognizerService
    {
        Task<Harvest> Recognize(string filePathName);
    }
}
