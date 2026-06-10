namespace NestVault.Api.Enums
{
    public enum RefreshTokenFailureReason
    {
        None,
        NotFound,
        Expired,
        Revoked,
        Reused,
        Invalid
    }
}
