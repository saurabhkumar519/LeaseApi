using LeaseBackend.BAL.IServices;
using LeaseBackend.DAL.Models;
using LeaseBackend.Model.Common;
using LeaseBackend.Model.DTOs.UserDTO;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LeaseBackend.BAL.Services
{
    public class UserService: IUserService
    {
        private readonly TICKETTOKENContext context;

        public UserService(TICKETTOKENContext context)
        {
            this.context=context;
        }


        public async Task<ResponseModel> GetAllRole()
        {
            var response = new ResponseModel();

            var roles = await context.Roles
                .AsNoTracking()
                .Select(r => new 
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToListAsync();

            response.StatusCode = HttpStatusCode.OK;
            response.Message = roles.Any() ? "Success" : "No roles found.";
            response.Data = roles;

            return response;
        }
        public async Task<ResponseModel> AssignEmployeeRoleAsync(AssignRoleRequest dto,long LoginUserId)
        {
            var response = new ResponseModel();

           
            if (dto.EmployeeId <= 0 || dto.UnitId <= 0 || dto.RoleId <= 0)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "EmployeeId, UnitId & RoleId are required.";
                return response;
            }

            
            var empExists = await context.MstEmployeeMasters
                .AsNoTracking()
                .AnyAsync(x => x.EmployeeMasterAutoId == dto.EmployeeId);

            if (!empExists)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Employee not found.";
                return response;
            }

            
            var unitExists = await context.MstUnits
                .AsNoTracking()
                .AnyAsync(x => x.Unitid == dto.UnitId);

            if (!unitExists)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Unit not found.";
                return response;
            }

           
            var roleExists = await context.Roles
                .AsNoTracking()
                .AnyAsync(x => x.RoleId == dto.RoleId);

            if (!roleExists)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Role not found.";
                return response;
            }

          
            bool alreadyAssigned = await context.EmployeeUnitRoles
                .AnyAsync(x =>
                    x.EmployeeId == dto.EmployeeId &&
                    x.UnitId == dto.UnitId &&
                    x.RoleId == dto.RoleId);

            if (alreadyAssigned)
            {
                response.StatusCode = HttpStatusCode.Conflict;
                response.Message = "Role already assigned to this employee in this unit.";
                return response;
            }

           
            var entity = new EmployeeUnitRole
            {
                EmployeeId = dto.EmployeeId,
                UnitId = dto.UnitId,
                RoleId = dto.RoleId,
                CreatedBy = Convert.ToInt32(LoginUserId),
                CreatedDate = DateTime.Now
            };

            await context.EmployeeUnitRoles.AddAsync(entity);
            await context.SaveChangesAsync();

           
            response.StatusCode = HttpStatusCode.Created;
            response.Message = "Role assigned successfully.";
            response.Data = entity;

            return response;
        }
        public async Task<ResponseModel> GetRolesByEmployeeSimpleDTOAsync(long? employeeId = null)
        {
            var response = new ResponseModel();

            var roles = await context.EmployeeUnitRoles
                .Include(x => x.Unit)
                .Include(x => x.Role)
                .Include(x => x.Employee)
                .Where(x => employeeId == null || x.EmployeeId == employeeId)
                .Select(x => new RoleAssignmentDTO
                {
                    Id = x.Id,
                    AssignedUnitId = x.UnitId,
                    AssignedUnitName = x.Unit != null ? x.Unit.UnitName : string.Empty,

                    AssignedRoleId = x.RoleId,
                    AssignedRoleName = x.Role != null ? x.Role.RoleName : string.Empty,

                    employeeDetails = x.Employee == null ? null : new EmployeeDTO
                    {
                        EmployeeMasterAutoId = x.Employee.EmployeeMasterAutoId,
                        EmployeeCode = x.Employee.EmployeeCode,
                        UserName = x.Employee.UserName,
                        DeptDfccil = x.Employee.DeptDfccil,
                        Post = x.Employee.Post,
                        UnitName = x.Employee.Location,
                        PositionGrade = x.Employee.PositionGrade
                    }
                })
                .ToListAsync();

            response.StatusCode = HttpStatusCode.OK;
            response.Message = roles.Any() ? "Success" : "No roles found.";
            response.Data = roles;

            return response;
        }

        public async Task<ResponseModel> GetRolesByEmployeeAsync(long? employeeId = null)
        {
            var response = new ResponseModel();

            try
            {
               
                var rawList = await context.EmployeeUnitRoles
                    .Include(x => x.Unit)
                    .Include(x => x.Role)
                    .Include(x => x.Employee)
                    .Where(x => employeeId == null || x.EmployeeId == employeeId)
                    .Select(x => new
                    {
                        x.Id,
                        UnitId = x.UnitId,
                        UnitName = x.Unit != null ? x.Unit.UnitName : string.Empty,

                        RoleId = x.RoleId,
                        RoleName = x.Role != null ? x.Role.RoleName : string.Empty,

                        Employee = x.Employee == null ? null : new EmployeeDTO
                        {
                            EmployeeMasterAutoId = x.Employee.EmployeeMasterAutoId,
                            EmployeeCode = x.Employee.EmployeeCode,
                            UserName = x.Employee.UserName,
                            DeptDfccil = x.Employee.DeptDfccil,
                            Post = x.Employee.Post,
                            UnitName = x.Employee.Location,
                            PositionGrade = x.Employee.PositionGrade
                        }
                    })
                    .ToListAsync();


               
                var groupedData = rawList
                    .GroupBy(e => new
                    {
                        e.Employee.EmployeeMasterAutoId,
                        e.Employee.EmployeeCode,
                        e.Employee.UserName,
                        e.Employee.DeptDfccil,
                        e.Employee.Post,
                        e.Employee.UnitName,
                        e.Employee.PositionGrade
                    })
                    .Select(empGroup => new
                    {
                        Employee = new
                        {
                            empGroup.Key.EmployeeMasterAutoId,
                            empGroup.Key.EmployeeCode,
                            empGroup.Key.UserName,
                            empGroup.Key.DeptDfccil,
                            empGroup.Key.Post,
                            empGroup.Key.UnitName,
                            empGroup.Key.PositionGrade
                        },

                        Roles = empGroup
                            .GroupBy(r => new { r.RoleId, r.RoleName })
                            .Select(roleGroup => new
                            {
                                RoleId = roleGroup.Key.RoleId,
                                RoleName = roleGroup.Key.RoleName,
                                Units = roleGroup.Select(u => new
                                {
                                    u.UnitId,
                                    u.UnitName
                                }).ToList()
                            }).ToList()
                    })
                    .ToList();


               
                response.StatusCode = HttpStatusCode.OK;
                response.Message = groupedData.Any() ? "Success" : "No roles found.";
                response.Data = groupedData;

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }
        }


        public async Task<ResponseModel> EditEmployeeRoleAsync(EditEmployeeRoleRequest dto,long loginUserId)
        {
            var response = new ResponseModel();

            if (dto.AssignmentId <= 0 || dto.NewUnitId <= 0 || dto.NewRoleId <= 0)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "AssignmentId, NewUnitId and NewRoleId are required.";
                return response;
            }

            var record = await context.EmployeeUnitRoles
                .FirstOrDefaultAsync(x => x.Id == dto.AssignmentId);

            if (record == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Assignment not found.";
                return response;
            }

          
            bool unitExists = await context.MstUnits
                .AsNoTracking()
                .AnyAsync(x => x.Unitid == dto.NewUnitId);

            if (!unitExists)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "New unit does not exist.";
                return response;
            }

           
            bool roleExists = await context.Roles
                .AsNoTracking()
                .AnyAsync(x => x.RoleId == dto.NewRoleId);

            if (!roleExists)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "New role does not exist.";
                return response;
            }

           
            bool isDifferent =
                record.UnitId != dto.NewUnitId ||
                record.RoleId != dto.NewRoleId;

            if (!isDifferent)
            {
                response.StatusCode = HttpStatusCode.OK;
                response.Message = "No changes found.";
                return response;
            }

           
            bool duplicateExists = await context.EmployeeUnitRoles
                .AnyAsync(x =>
                    x.EmployeeId == record.EmployeeId &&
                    x.UnitId == dto.NewUnitId &&
                    x.RoleId == dto.NewRoleId &&
                    x.Id != dto.AssignmentId);

            if (duplicateExists)
            {
                response.StatusCode = HttpStatusCode.Conflict;
                response.Message = "This role & unit is already assigned to the employee.";
                return response;
            }
            
           
            record.UnitId = dto.NewUnitId;
            record.RoleId = dto.NewRoleId;
            record.CreatedBy =Convert.ToInt32(loginUserId);
            record.CreatedDate = DateTime.Now;

            await context.SaveChangesAsync();

            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Role assignment updated successfully.";
            response.Data = record;

            return response;
        }
        public async Task<ResponseModel> DeleteEmployeeRoleAsync(int Id)
        {
            var response = new ResponseModel();

            if (Id <= 0)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "AssignmentId is required.";
                return response;
            }

            var record = await context.EmployeeUnitRoles
                .FirstOrDefaultAsync(x => x.Id == Id);

            if (record == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Message = "Assignment not found.";
                return response;
            }

            context.EmployeeUnitRoles.Remove(record);
            await context.SaveChangesAsync();

            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Role removed successfully.";
            response.Data = record;

            return response;
        }

    }
}
