using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Interfaces
{
    public interface IStorageService
    {
        Task<string> Upload(string blobName, string filePathName);
    }
}
