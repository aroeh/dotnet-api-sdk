using Dotnet.API.MSSQL.SDK.Interfaces;
using Dotnet.API.MSSQL.SDK.Options;

namespace Dotnet.API.MSSQL.SDK.Repos;

public class ADOSqlRepo
(
    MSSqlOptions options
) : ADOSqlRepoBase(options), IADOSqlRepo
{
}
