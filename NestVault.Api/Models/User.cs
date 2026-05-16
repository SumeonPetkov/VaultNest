using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NestVault.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    }
}
