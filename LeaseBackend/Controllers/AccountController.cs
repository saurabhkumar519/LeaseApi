using LeaseBackend.API.ActionFilter;
using LeaseBackend.BAL.Extension;
using LeaseBackend.BAL.IServices;
using LeaseBackend.Model.Common;
using LeaseBackend.Model.DTOs.UserDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace LeaseBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class AccountController : ControllerBase
    {
        public readonly IAccountService _accountRepository;
        private readonly IConfiguration _configuration;
        private readonly ICurrentUserService currentUserService;

        public AccountController(IAccountService accountRepository, IConfiguration configuration,ICurrentUserService currentUserService)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
            this.currentUserService=currentUserService;
        }



        [HttpGet("profile")]
        public async Task<ResponseModel> GetUserProfile()
        {
            var responseModel = new ResponseModel();

            try
            {
                var currentUser = HttpContext.Items["EmployeeCode"];

                if (currentUser == null)
                {
                    responseModel.StatusCode = HttpStatusCode.Unauthorized;
                    responseModel.Message = "User not found in context";
                    return responseModel;
                }

                var empCode = Convert.ToString(currentUser) ?? string.Empty;

               
                var employeeDetails = await _accountRepository.GetEmployeeDetailsAsync(empCode);
                var user = currentUserService.GetCurrentUser();

                if (employeeDetails == null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "Employee details not found";
                    return responseModel;
                }

                bool isSuperAdmin = _configuration["SuperAdmin"] == employeeDetails.EmployeeCode;

                
                List<object> rolesOutput = new();

                if (isSuperAdmin)
                {
                    rolesOutput.Add(new
                    {
                        RoleName = "SuperAdmin",
                        Units = new List<object>
                {
                    new { UnitId = (int?)null, UnitName = "All" }
                }
                    });
                }
                else
                {
                   
                    var roles = await currentUserService.GetEmployeeRolesWithUnitsAsync(employeeDetails.EmployeeMasterAutoId);

                    if (roles != null && roles.Any())
                    {
                        rolesOutput = roles
                            .Select(r => new
                            {
                                RoleName = r.RoleName ?? "Unknown",
                                Units = r.Units
                                    ?.Select(u => new
                                    {
                                        UnitId = u.UnitId,
                                        UnitName = u.UnitName ?? "Unknown"
                                    })
                                    .ToList()
                            })
                            .ToList<object>();
                    }
                }

              
                var userProfile = new
                {
                    Employee = new
                    {
                        employeeDetails.EmployeeMasterAutoId,
                        employeeDetails.EmployeeCode,
                        employeeDetails.UserName,
                        employeeDetails.DeptDfccil,
                        employeeDetails.Post,
                        Location = employeeDetails.Location ?? "",
                        employeeDetails.PositionGrade
                    },
                    Roles = rolesOutput
                };

                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Profile retrieved successfully";
                responseModel.Data = userProfile;
            }
            catch (Exception)
            {
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                responseModel.Message = "An error occurred while retrieving profile";
            }

            return responseModel;
        }





        [HttpGet("user-roles/{EmployeeId}")]
        public async Task<ResponseModel> GetUserRoles(int EmployeeId)
        {
            var responseModel = new ResponseModel();

            try
            {
                var roles = await currentUserService.GetEmployeeRolesWithUnitsAsync(EmployeeId);

                responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                responseModel.Message = "Roles retrieved successfully";
                responseModel.Data = new { Roles = roles };
            }
            catch (Exception ex)
            {
                responseModel.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                responseModel.Message = "An error occurred while retrieving roles";
            }

            return responseModel;
        }

    }
}
