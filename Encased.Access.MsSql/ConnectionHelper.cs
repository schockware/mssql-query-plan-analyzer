using Azure.Identity;
using Microsoft.Data.SqlClient;

namespace Encased.Access.MsSql;

public static class ConnectionHelper
{
    public static async Task<SqlConnection> GetConnection(string connectionString)
    {
        var conn = new SqlConnection(connectionString);
        if (!connectionString.Contains("Password"))
            conn.AccessToken = await GetToken();
        return conn;
    }

    private static async Task<string> GetToken()
    {
        var cred = new DefaultAzureCredential();
        var token = await cred.GetTokenAsync(new Azure.Core.TokenRequestContext(["https://database.windows.net/"]));

        return token.Token;
    }
}