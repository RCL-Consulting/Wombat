using Microsoft.EntityFrameworkCore;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Users;

/// <summary>
/// An empty in-memory database for the Users handlers that read one only to judge a scope (T376, M2: Add role reads the
/// person's institution's adopted disciplines). A refusal asked before that read never touches it.
/// </summary>
internal static class UsersTestDb
{
    public static ApplicationDbContext Empty()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
