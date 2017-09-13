using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Demo.Web.Hubs;
using Demo.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers
{
    [Route("api/users")]
    public class UserController : Controller
    {
        private readonly IIdentityService mIdentityService;
        private readonly UserManager<IdentityUser> mUserManager;

        #region Constructor

        public UserController(IIdentityService identityService, UserManager<IdentityUser> userManager)
        {
            mIdentityService = identityService;
            mUserManager = userManager;
        }

        #endregion

        [HttpGet]
        [Authorize]
        [Route("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await mIdentityService.GetUser(this.User);

                if (user == null)
                {
                    return NotFound();
                }

                return await AsResponse(new
                {
                    data = user
                });
            }
            catch (Exception ex)
            {

                return await AsResponse(new
                {
                    error = ex.Message
                }, HttpStatusCode.InternalServerError);

            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            try
            {

                return await AsResponse(new
                {
                    data = mUserManager.Users.ToList()
                });
            }
            catch (Exception ex)
            {

                return await AsResponse(new
                {
                    error = ex.Message
                }, HttpStatusCode.InternalServerError);

            }
        }

        private Task<IActionResult> AsResponse(object data, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return Task.FromResult<IActionResult>(new ObjectResult(data) { StatusCode = (int)statusCode });
        }
    }
}
