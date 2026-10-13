using LeaseBackend.BAL.Extension;
using LeaseBackend.BAL.IServices;
using LeaseBackend.DAL.Models;
using LeaseBackend.Model.Common;
using LeaseBackend.Model.DTOs.UserDTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseBackend.BAL.Services
{
    public class AccountRepository: IAccountService
    {
        private readonly TICKETTOKENContext context;
        private readonly IUserService userService;
        private readonly ICurrentUserService currentUser;

        public AccountRepository(TICKETTOKENContext context,IUserService userService, ICurrentUserService currentUser)
        {
            this.context=context;
            this.userService=userService;
            this.currentUser=currentUser;
        }

        public async Task<MstEmployeeMaster> GetEmployeeDetailsAsync(string empCode)
        {
            return await context.MstEmployeeMasters.FirstOrDefaultAsync(e => e.EmployeeCode == empCode)?? new MstEmployeeMaster();
        }

        

    }
}
