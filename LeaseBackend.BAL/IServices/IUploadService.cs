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
    public interface IUploadService
    {
        Task<ResponseModel> SaveExcelUploadAsync(UploadExcelRequestDTO request);
        Task<ResponseModel> GetElectricityEntriesAsync(int roleId, UserTrackingEnum trackingEnum);
        Task<ResponseModel> GetLeaseEntriesAsync(int roleId, UserTrackingEnum trackingEnum);
        Task<ResponseModel> UpdateUploadStatusAsync(UpdateStatusDTO dto);
        Task<ResponseModel> GetMasterUnit();
        Task<ResponseModel> GetMasterEmployee(List<int>? unitIds = null);

    }
}
