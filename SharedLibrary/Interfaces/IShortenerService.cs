using System;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IShortenerService
    {
        Task<string> DoShort(string longUrl);
    }
}
