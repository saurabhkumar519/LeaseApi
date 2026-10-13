using LeaseBackend.DAL.Models;
using LeaseBackend.Model.Common;
using LeaseBackend.Model.DTOs.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LeaseBackend.BAL.IServices
{
    public interface IUserService
    {
        Task<ResponseModel> GetAllRole();
        Task<ResponseModel> AssignEmployeeRoleAsync(AssignRoleRequest dto, long LoginUserId);
        Task<ResponseModel> GetRolesByEmployeeSimpleDTOAsync(long? employeeId = null);
        Task<ResponseModel> GetRolesByEmployeeAsync(long? employeeId = null);
        Task<ResponseModel> EditEmployeeRoleAsync(EditEmployeeRoleRequest dto, long loginUserId);
        Task<ResponseModel> DeleteEmployeeRoleAsync(int Id);
        
    }
}
