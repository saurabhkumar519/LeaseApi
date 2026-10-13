using LeaseBackend.BAL.IServices;
using LeaseBackend.DAL.Models;
using LeaseBackend.Model.DTOs.UserDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static LeaseBackend.BAL.Extension.CurrentUserService;

namespace LeaseBackend.BAL.Extension
{
    public interface ICurrentUserService
    {
        EmployeeDTO GetCurrentUser();
        Task<List<RoleWithUnitsDTO>> GetEmployeeRolesWithUnitsAsync(long employeeMasterAutoId);
    }
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TICKETTOKENContext context;
        private readonly IUserService userService;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, TICKETTOKENContext context,IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            this.context=context;
            this.userService=userService;
        }

        public EmployeeDTO GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext.Items.TryGetValue("EmployeeCode", out var empCode))
            {
                var employeeCode = empCode?.ToString();
                
                var employee = context.MstEmployeeMasters
                    .Where(e => e.EmployeeCode == employeeCode)
                    .Select(x => new EmployeeDTO
                    {
                        EmployeeMasterAutoId = x.EmployeeMasterAutoId,
                        EmployeeCode = x.EmployeeCode,
                        UserName = x.UserName,
                        Post = x.Post,
                        PositionGrade = x.PositionGrade,
                        DeptDfccil = x.DeptDfccil,
                        UnitName = x.Location,
                        Phone = x.Mobile,
                        Email = x.EmailAddress,

                        UnitId = context.MstUnits
                            .Where(y => y.UnitName.ToLower() == x.Location.ToLower())
                            .Select(y => (int?)y.Unitid)
                            .FirstOrDefault() ?? 0
                    })
                    .FirstOrDefault();

                return employee ?? new EmployeeDTO();
            }

            return new EmployeeDTO();
        }

        public async Task<List<RoleWithUnitsDTO>> GetEmployeeRolesWithUnitsAsync(long employeeMasterAutoId)
        {
            var result = new List<RoleWithUnitsDTO>();

            if (employeeMasterAutoId <= 0)
                return result;

            var roles = await context.EmployeeUnitRoles
                .Where(x => x.EmployeeId == employeeMasterAutoId)
                .Include(x => x.Role)
                .Include(x => x.Unit)
                .Select(x => new
                {
                    RoleId= x.RoleId,
                    RoleName = x.Role != null ? x.Role.RoleName : null,
                    UnitId = x.UnitId,
                    UnitName = x.Unit != null ? x.Unit.UnitName : null
                })
                .ToListAsync();

            if (roles == null || !roles.Any())
                return result;

            result = roles
                .Where(x => x.RoleName != null)
                .GroupBy(x => new {x.RoleId,x.RoleName})
                .Select(g => new RoleWithUnitsDTO
                {
                    RoleId = g.Key.RoleId,
                    RoleName = g.Key.RoleName,
                    Units = g
                        .GroupBy(u => new { u.UnitId, u.UnitName })
                        .Select(u => new UnitDTO
                        {
                            UnitId = u.Key.UnitId,
                            UnitName = u.Key.UnitName
                        })
                        .ToList()
                })
                .ToList();

            return result;
        }

        public class RoleWithUnitsDTO
        {
            public int RoleId { get; set; }
            public string? RoleName { get; set; }
            public List<UnitDTO> Units { get; set; } = new();
        }

        public class UnitDTO
        {
            public int? UnitId { get; set; }
            public string? UnitName { get; set; }
        }

    }

}
