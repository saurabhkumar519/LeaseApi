using LeaseBackend.BAL.IServices;
using LeaseBackend.BAL.Services;
using LeaseBackend.Model.DTOs.UserDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LeaseBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService uploadService;

        public UploadController(IUploadService uploadService)
        {
            this.uploadService=uploadService;
        }


        [HttpPost("GetUnits")]
        public async Task<IActionResult> GetUnits()
        {
            var response = await uploadService.GetMasterUnit();
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpGet("GetEmployee")]
        public async Task<IActionResult> GetEmployee([FromQuery] List<int> UnitId)
        {
            var response = await uploadService.GetMasterEmployee(UnitId);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("FinalSubmit")]
        public async Task<IActionResult> SaveExcelUpload([FromBody] UploadExcelRequestDTO request)
        {
            var response = await uploadService.SaveExcelUploadAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        
        [HttpGet("TrackRequestLease")]
        public async Task<IActionResult> TrackRequestLease(int RoleId, UserTrackingEnum status )
        {
            var response = await uploadService.GetLeaseEntriesAsync(RoleId, status);
            return StatusCode((int)response.StatusCode, response);
        }
        
        [HttpGet("TrackRequestElectricity")]
        public async Task<IActionResult> TrackRequestElectricity(int RoleId, UserTrackingEnum status)
        {
            var response = await uploadService.GetElectricityEntriesAsync(RoleId, status);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("updateStatus")]
        public async Task<IActionResult> UpdateUploadStatus([FromBody] UpdateStatusDTO dto)
        {
            var response = await uploadService.UpdateUploadStatusAsync(dto);
            return StatusCode((int)response.StatusCode, response);
        }
        [AllowAnonymous]
        [HttpGet("download-sample/{fileName}")]
        public IActionResult DownloadSample(string fileName)
        {
            try
            {
                var folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "LeaseAndElectricityRecovery"
                );

                var filePath = Path.Combine(folderPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Sample file not found.");
                }

                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return PhysicalFile(
                    filePath,
                    contentType,
                    fileName
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}

