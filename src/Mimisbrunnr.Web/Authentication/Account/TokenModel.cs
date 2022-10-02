namespace Mimisbrunnr.Web.Authentication.Account;

public class TokenModel
{
    public string Id { get; set; }

    public DateTime Created { get; set; }

    public DateTime Expired { get; set; }

    public bool Revoked { get; set; }
}