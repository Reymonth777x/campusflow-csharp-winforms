using System;
using System.Collections.Generic;
using System.Text;
namespace CampusFlow.Infrastructure.Persistence;

internal static class DatabasePaths
{
    public static string Default => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CampusFlow",
        "campusflow.db");
}
