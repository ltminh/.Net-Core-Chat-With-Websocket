using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Demo.Core.DataContext
{
    public interface IDbInitializer
    {
        void Initialize();
    }

    public class DbInitializer : IDbInitializer
    {
        private readonly DataContext mContext;
        private readonly UserManager<IdentityUser> mUserManager;


        public DbInitializer(
            DataContext context,
            UserManager<IdentityUser> userManager)
        {
            mContext = context;
            mUserManager = userManager;
        }

        public async void Initialize()
        {
            //create database schema if none exists
            mContext.Database.EnsureCreated();

            #region Seed Data

            if (!mContext.Set<IdentityUser>().Any())
            {
                await mUserManager.CreateAsync(new IdentityUser { UserName = "user1@g.com", Email = "use1@g.com", EmailConfirmed = true }, "123456");
                await mUserManager.CreateAsync(new IdentityUser { UserName = "user2@g.com", Email = "use2@g.com", EmailConfirmed = true }, "123456");
                await mUserManager.CreateAsync(new IdentityUser { UserName = "user3@g.com", Email = "use3@g.com", EmailConfirmed = true }, "123456");
            }

            #endregion

        }
    }
}
