using LeaseBackend.BAL.Extension;
using LeaseBackend.BAL.IServices;
using LeaseBackend.Model.Common;
using LeaseBackend.Model.DTOs.UserDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;

namespace LeaseBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ICurrentUserService currentUserService;
        private EmployeeDTO currentUser;
        public UserController(IUserService userService,ICurrentUserService currentUserService)
        {
            this.userService=userService;
            this.currentUserService=currentUserService;
           
            this.currentUser = currentUserService.GetCurrentUser();
        }


        [HttpGet("AllRoles")]
        public async Task<ResponseModel> GetAssignedRoles()
        {
            var result = await userService.GetAllRole();
            return result;
        }
        [AllowAnonymous]
        [HttpGet("GetAssignedRoles")]
        public async Task<ResponseModel> GetAssignedRoles(long? employeeId = null)
        {
            var result = await userService.GetRolesByEmployeeAsync(employeeId);
            return result;
        }
        [HttpPut("AddRoleAssignment")]
        public async Task<ResponseModel> AddRoleAssignment([FromBody] AssignRoleRequest request)
        {
            var result = await userService.AssignEmployeeRoleAsync(request,currentUser.EmployeeMasterAutoId);
            return result;
        }


        [HttpPut("EditRoleAssignment")]
        public async Task<ResponseModel> EditRoleAssignment([FromBody] EditEmployeeRoleRequest request)
        {
            var result = await userService.EditEmployeeRoleAsync(request,currentUser.EmployeeMasterAutoId);
            return result;
        }


        [HttpDelete("DeleteRoleAssignment/{id}")]
        public async Task<ResponseModel> DeleteRoleAssignment(int id)
        {
            var result = await userService.DeleteEmployeeRoleAsync(id);
            return result;
        }
    }

}
