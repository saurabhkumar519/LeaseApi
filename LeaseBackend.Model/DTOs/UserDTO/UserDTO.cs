using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LeaseBackend.Model.DTOs.UserDTO
{
    public class AssignRoleRequest
    {
        public long EmployeeId { get; set; }
        public int UnitId { get; set; }
        public int RoleId { get; set; }
    }


    public class EditEmployeeRoleRequest
    {
        public int AssignmentId { get; set; }
        public int NewUnitId { get; set; }
        public int NewRoleId { get; set; }
    }

    public class EmployeeDTO
    {
        public long EmployeeMasterAutoId { get; set; }
        public string EmployeeCode { get; set; }
        public string UserName { get; set; }
        public string Post { get; set; }
        public string PositionGrade { get; set; }
        public string DeptDfccil { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

    }
    public class RoleAssignmentDTO
    {
        public int Id { get; set; }
        public int AssignedUnitId { get; set; }
        public string AssignedUnitName { get; set; }
        public int AssignedRoleId { get; set; }
        public string AssignedRoleName { get; set; }
        public EmployeeDTO? employeeDetails { get; set; }
    }
    public class ElectricityEntryExcelDto
    {
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal BillAmount { get; set; }
    }

    public class LeaseRecoveryExcelDto
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string? Designation { get; set; }
        public string? QuarterType { get; set; }
        public DateTime? AllottedOn { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal LeaseRecoveryAmount { get; set; }
        public string? CityCategory { get; set; }
    }

    public class UploadExcelRequestDTO
    {
        public int UnitId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EntryType FileType { get; set; }
        public List<ElectricityEntryExcelDto>? ElectricityEntries { get; set; }
        public List<LeaseRecoveryExcelDto>? LeaseRecoveries { get; set; }
    }

    public class UpdateStatusDTO
    {
        
        public List<int> EntryIds { get; set; }= new List<int>();
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EntryType? EntryType { get; set; }
        public statusEnum Status { get; set; }
        public string? Remarks { get; set; }
    }
    public enum EntryType
    {
        ElectricityEntry,
        LeaseRecovery
    }
    public enum statusEnum
    {
        Save = 1,
        ApproveByVerifier1 = 2,
        ApproveByVerifier2 = 3,
        rejectByVerifier1 = 99,
        rejectByVerifier2 = 999
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserTrackingEnum
    {
        Pending,
        Rejected,
        Approved

    }

}
