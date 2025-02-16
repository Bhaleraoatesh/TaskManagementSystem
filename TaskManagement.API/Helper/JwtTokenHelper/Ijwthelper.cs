namespace TaskManagement.API.Helper.JwtTokenHelper
{
    public interface Ijwthelper
    {
        string GenerateToken(string username, string role);
    }
}
