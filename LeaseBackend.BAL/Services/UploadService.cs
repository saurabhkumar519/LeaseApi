using LeaseBackend.BAL.Extension;
using LeaseBackend.BAL.IServices;
using LeaseBackend.DAL.Models;
using LeaseBackend.Model.Common;
using LeaseBackend.Model.DTOs.UserDTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;

namespace LeaseBackend.BAL.Services
{
    public class UploadService: IUploadService
    {
        private readonly TICKETTOKENContext context;
        private readonly ICurrentUserService _CService;
        private readonly EmployeeDTO CurrentUser;
        public UploadService(TICKETTOKENContext context,ICurrentUserService CService)
        {
            this.context=context;
            this._CService=CService;
            CurrentUser=_CService.GetCurrentUser();
        }
        public async Task<ResponseModel> SaveExcelUploadAsync(UploadExcelRequestDTO request)
        {
            var response = new ResponseModel();
            if (!Enum.IsDefined(typeof(EntryType), request.FileType))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Invalid FileType value.";
                return response;
            }

            var AlloWstatus = new List<int> {99,999};
            try
            {
               
                var master = new UploadsMaster
                {
                    UnitId = request.UnitId,
                    Status = 1,
                    CreatedDate = DateTime.Now,
                    CreatedBy = CurrentUser.EmployeeCode,
                    Remarks = "Uploaded from Excel",
                    FileType = request.FileType.ToString()
                };

               
                if (request.FileType == EntryType.ElectricityEntry)
                {
                    if (request.ElectricityEntries == null || !request.ElectricityEntries.Any())
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = "Electricity file contains no valid rows.";
                        return response;
                    }

                    foreach (var item in request.ElectricityEntries)
                    {
                        bool exists = await context.ElectricityEntries.AnyAsync(e =>
                            e.EmployeeCode == item.EmployeeCode &&
                            e.Year == item.Year &&
                            e.Month == item.Month && e.Status.HasValue &&
                            !AlloWstatus.Contains(e.Status.Value)
                        );

                        if (exists)
                        {
                            response.StatusCode = HttpStatusCode.BadRequest;
                            response.Message = $"Approved Electricity record already exists for EmployeeCode {item.EmployeeCode} ({item.Month}/{item.Year}).";
                            return response;
                        }
                    }

                    
                    foreach (var item in request.ElectricityEntries)
                    {
                        master.ElectricityEntries.Add(new ElectricityEntry
                        {
                            EmployeeCode = item.EmployeeCode,
                            Name = item.Name,
                            Designation = item.Designation,
                            Year = item.Year,
                            UnitId=request.UnitId,
                            Month = item.Month,
                            BillAmount = item.BillAmount,
                            CreatedDate = DateTime.Now,
                            Status = 1
                        });
                    }
                }

               
                if (request.FileType == EntryType.LeaseRecovery)
                {
                    if (request.LeaseRecoveries == null || !request.LeaseRecoveries.Any())
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = "Lease recovery file contains no valid rows.";
                        return response;
                    }

                    foreach (var item in request.LeaseRecoveries)
                    {
                        bool exists = await context.LeaseRecoveries.AnyAsync(e =>
                            e.EmployeeCode == item.EmployeeCode &&
                            e.Year == item.Year &&
                            e.Month == item.Month && e.Status.HasValue &&
                            !AlloWstatus.Contains(e.Status.Value)
                        );

                        if (exists)
                        {
                            response.StatusCode = HttpStatusCode.BadRequest;
                            response.Message = $"Approved Lease Recovery already exists for EmployeeCode {item.EmployeeCode} ({item.AllottedOn:MM/yyyy}).";
                            return response;
                        }
                    }

                   
                    foreach (var item in request.LeaseRecoveries)
                    {
                        master.LeaseRecoveries.Add(new LeaseRecovery
                        {
                            EmployeeCode = item.EmployeeCode,
                            EmployeeName = item.EmployeeName,
                            Designation = item.Designation,
                            QuarterType = item.QuarterType,
                            AllottedOn = item.AllottedOn,
                            Year= item.Year,
                            Month= item.Month,
                            UnitId=request.UnitId,
                            LeaseRecoveryAmount = item.LeaseRecoveryAmount,
                            CityCategory = item.CityCategory,
                            Status = 1
                        });
                    }
                }

               
                await context.UploadsMasters.AddAsync(master);
                await context.SaveChangesAsync();

                response.StatusCode = HttpStatusCode.Created;
                response.Data = new { UploadId = master.Id };
                response.Message = $"{request.FileType} Excel uploaded successfully.";

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }
        }


        public async Task<ResponseModel> GetElectricityEntriesAsync(int roleId,UserTrackingEnum trackingEnum)
        {
            var response = new ResponseModel();

            try
            {
               
                if (!StatusMap.TryGetValue(roleId, out var statusSet))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = "Invalid role.";
                    return response;
                }

               
                var permittedUnitIds = await GetPermittedUnitIdsAsync(roleId);

                if (!permittedUnitIds.Any())
                {
                    response.StatusCode = HttpStatusCode.Forbidden;
                    response.Message = "You are not assigned to any unit.";
                    return response;
                }

                
                var statusFilter = trackingEnum switch
                {
                    UserTrackingEnum.Pending => statusSet.Pending,
                    UserTrackingEnum.Approved => statusSet.Approved,
                    UserTrackingEnum.Rejected => statusSet.Rejected,
                    _ => Array.Empty<int>()
                };

                
                var data = await (
                    from e in context.ElectricityEntries.AsNoTracking()
                    join emp in context.MstEmployeeMasters.AsNoTracking()
                        on e.UpdatedBy equals emp.EmployeeCode into empJoin
                    from emp in empJoin.DefaultIfEmpty()
                    where e.UnitId.HasValue
                          && permittedUnitIds.Contains(e.UnitId.Value)
                          && e.Status.HasValue
                          && statusFilter.Contains(e.Status.Value)
                    orderby e.UnitId, e.UploadId
                    select new
                    {
                        ElectricityEntryId = e.Id,
                        e.UploadId,
                        e.UnitId,
                        e.EmployeeCode,
                        e.Name,
                        e.Designation,
                        e.Year,
                        e.Month,
                        e.BillAmount,
                        e.Status,
                        e.CreatedDate,
                        e.UpdatedDate,
                        e.UpdatedBy,
                        UpdatedByName = emp != null ? emp.UserName : null,
                        e.Remarks
                    }
                ).ToListAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Data = data;
                response.Message = "Electricity entries fetched successfully.";
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }
        }


        public async Task<ResponseModel> GetLeaseEntriesAsync(int roleId, UserTrackingEnum trackingEnum)
        {
            var response = new ResponseModel();

            try
            {
                if (!StatusMap.TryGetValue(roleId, out var statusSet))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = "Invalid role.";
                    return response;
                }

                var permittedUnitIds = await GetPermittedUnitIdsAsync(roleId);

                if (!permittedUnitIds.Any())
                {
                    response.StatusCode = HttpStatusCode.Forbidden;
                    response.Message = "You are not assigned to any unit.";
                    return response;
                }

                var employeeNames = await context.MstEmployeeMasters
                    .AsNoTracking()
                    .Where(x => x.EmployeeCode != null && x.Status == 0)
                    .ToDictionaryAsync(x => x.EmployeeCode!, x => x.UserName);

                IQueryable<LeaseRecovery> query = context.LeaseRecoveries
                    .AsNoTracking()
                    .Where(l => l.UnitId.HasValue &&
                                permittedUnitIds.Contains(l.UnitId.Value));

                var statusFilter = trackingEnum switch
                {
                    UserTrackingEnum.Pending => statusSet.Pending,
                    UserTrackingEnum.Approved => statusSet.Approved,
                    UserTrackingEnum.Rejected => statusSet.Rejected,
                    _ => Array.Empty<int>()
                };

                query = query.Where(x => x.Status.HasValue &&
                                         statusFilter.Contains(x.Status.Value));

                var list = await query
                            .OrderBy(x => x.UnitId)
                            .ThenBy(x => x.UploadId)
                            .ToListAsync();
                            
                var data = list.Select(l => new
                {
                    LeaseEntryId = l.Id,
                    l.UploadId,
                    l.UnitId,
                    l.EmployeeCode,
                    l.EmployeeName,
                    l.Designation,
                    l.QuarterType,
                    l.AllottedOn,
                    l.LeaseRecoveryAmount,
                    l.CityCategory,
                    l.Month,
                    l.Year,
                    l.Status,
                    l.CreatedAt,
                    l.UpdatedDate,
                    l.UpdatedBy,
                    UpdatedByName = l.UpdatedBy != null &&
                                    employeeNames.TryGetValue(l.UpdatedBy, out var name)
                                        ? name
                                        : null,
                    l.Remarks
                }).ToList();


                response.StatusCode = HttpStatusCode.OK;
                response.Data = data;
                response.TotalRecords = data.Count;
                response.Message = "Lease entries fetched successfully.";
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }
        }
        public async Task<ResponseModel> UpdateUploadStatusAsync(UpdateStatusDTO dto)
        {
            var response = new ResponseModel();

            if (dto.EntryType == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "EntryType is required.";
                return response;
            }

            if (dto.EntryIds == null || !dto.EntryIds.Any())
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "At least one EntryId is required.";
                return response;
            }

            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                switch (dto.EntryType)
                {
                    case EntryType.ElectricityEntry:
                        {
                            var records = await context.ElectricityEntries
                                .Where(x => dto.EntryIds.Contains(x.Id))
                                .ToListAsync();

                            if (!records.Any())
                            {
                                response.StatusCode = HttpStatusCode.NotFound;
                                response.Message = "Electricity entries not found.";
                                return response;
                            }

                            foreach (var record in records)
                            {
                                record.Status = (int)dto.Status;
                                record.Remarks = dto.Remarks;
                                record.UpdatedDate = DateTime.Now;
                                record.UpdatedBy = CurrentUser.EmployeeCode;
                            }

                            break;
                        }

                    case EntryType.LeaseRecovery:
                        {
                            var records = await context.LeaseRecoveries
                                .Where(x => dto.EntryIds.Contains(x.Id))
                                .ToListAsync();

                            if (!records.Any())
                            {
                                response.StatusCode = HttpStatusCode.NotFound;
                                response.Message = "Lease recovery entries not found.";
                                return response;
                            }

                            foreach (var record in records)
                            {
                                record.Status = (int)dto.Status;
                                record.Remarks = dto.Remarks;
                                record.UpdatedDate = DateTime.Now;
                                record.UpdatedBy = CurrentUser.EmployeeCode;
                            }

                            break;
                        }

                    default:
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = "Invalid EntryType.";
                        return response;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Status updated successfully.";
                response.Data = dto.EntryIds;
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }
        }



        public async Task<ResponseModel> GetMasterUnit()
        {
            var response = new ResponseModel();

            try
            {
                var mstunit= await context.MstUnits.Where(x=>x.Status==0)
                    .AsNoTracking()
                    .Select(x => new 
                    {
                        UnitId = x.Unitid,
                        UnitName = x.UnitName
                    })
                    .ToListAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Data = mstunit;
                response.Message = "Records fetched successfully.";

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }
        }
        public async Task<ResponseModel> GetMasterEmployee(List<int>? unitIds = null)
        {
            var response = new ResponseModel();

            try
            {
                List<string>UnitName= new List<string>();
                if (unitIds != null && unitIds.Count>0)
                {
                    UnitName= context.MstUnits
                        .AsNoTracking()
                        .Where(x => unitIds.Contains(x.Unitid))
                        .Select(x => x.UnitName.ToLower())
                        .ToList();
                }
                var query = context.MstEmployeeMasters
                    .AsNoTracking()
                    .Where(x => x.Status == 0);

               
                if (UnitName != null && UnitName.Any())
                {
                    query = query.Where(x => UnitName.Contains(x.Location.ToLower()));
                }

              
                var mstunit = await query
                    .Select(x => new
                    {
                        EmployeeId=x.EmployeeMasterAutoId,
                        EmployeeCode = x.EmployeeCode,
                        EmployeeName = x.UserName,
                        Designation = x.Post,
                        Location = x.Location,
                        Department=x.DeptDfccil
                    })
                    .ToListAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Data = mstunit;
                response.Message = "Records fetched successfully.";

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }
        }

        #region hepler

        private static readonly Dictionary<int, (int[] Pending, int[] Approved, int[] Rejected)>
             StatusMap = new()
          {
              { 1, (new[] { 1, 2 }, new[] { 3 }, new[] { 99, 999 }) },
              { 2, (new[] { 1 },   new[] { 2 }, new[] { 99 }) },
              { 3, (new[] { 2 },   new[] { 3 }, new[] { 999 }) }
          };
        private async Task<List<int>> GetPermittedUnitIdsAsync(int roleId)
        {
            var loginUser = CurrentUser;

            var currentUserRoles =
                await _CService.GetEmployeeRolesWithUnitsAsync(loginUser.EmployeeMasterAutoId);

            if (currentUserRoles == null || !currentUserRoles.Any())
                return new List<int>();

            return currentUserRoles
                .Where(x => x.RoleId == roleId)
                .SelectMany(r => r.Units)
                .Where(u => u.UnitId.HasValue)
                .Select(u => u.UnitId.Value)
                .Distinct()
                .ToList();
        }
        #endregion

    }
}
