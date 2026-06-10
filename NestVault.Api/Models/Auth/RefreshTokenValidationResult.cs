using NestVault.Api.Enums;

namespace NestVault.Api.Models.Auth
{
    public sealed class RefreshTokenValidationResult
    {
        public bool IsValid { get; init; }
        public Guid? UserId { get; init; }
        public Guid? SessionId { get; init; }
        public Guid? RefreshTokenId { get; init; }
        public RefreshTokenFailureReason FailureReason { get; init; }
    }
}