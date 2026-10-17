using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CampusFlow.Infrastructure.Persistence;

public sealed class CampusFlowDbContextFactory : IDesignTimeDbContextFactory<CampusFlowDbContext>
{
    public CampusFlowDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CampusFlowDbContext>()
            .UseSqlite("Data Source=campusflow.design.db")
            .Options;

        return new CampusFlowDbContext(options);
    }
}
