using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Demo.Core.DataContext;
using Microsoft.AspNetCore.Identity;

namespace Demo.Web.Services
{
    public interface IIdentityService
    {
        Task<IdentityUser> GetUser(ClaimsPrincipal userClaimsPrincipal);
    }

    public class IdentityService : IIdentityService
    {
        protected DataContext DataContext;


        public IdentityService(DataContext dataContext)
        {
            this.DataContext = dataContext;
        }

        public async Task<IdentityUser> GetUser(ClaimsPrincipal userClaimsPrincipal)
        {
            string userId = userClaimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var user = this.DataContext.Set<IdentityUser>().FirstOrDefault(us => us.Id == userId);

            return user;
        }
    }
}
