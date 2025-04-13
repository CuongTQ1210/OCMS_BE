using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface IPdfSignerService
    {
        Task<string> SignPdfAsync(string fileDataBase64);
    }
}
