using LeaseBackend.DAL.Models;
using LeaseBackend.Model.Common;
using LeaseBackend.Model.DTOs.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseBackend.BAL.IServices
{
    public interface IAccountService
    {
        Task<MstEmployeeMaster> GetEmployeeDetailsAsync(string empCode);

        
    }
}
