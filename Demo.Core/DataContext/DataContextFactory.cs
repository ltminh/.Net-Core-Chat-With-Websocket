using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Demo.Core.DataContext
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer("Server=192.168.1.222\\MSSQLSERVER2014,8181;Database=DemoChat;User ID=sa;Password=abc123;Trusted_Connection=False;MultipleActiveResultSets=true");

            return new DataContext(optionsBuilder.Options);
        }
    }
}
