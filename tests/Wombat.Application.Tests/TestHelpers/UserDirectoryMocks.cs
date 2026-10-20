using Moq;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Tests.TestHelpers;

/// <summary>What a mocked user directory needs to run the admission (T373 fix pass, B2).</summary>
public static class UserDirectoryMocks
{
    /// <summary>
    /// <see cref="IUserAdministrationService.InOneTransactionAsync{T}" /> runs its work, as the default implementation does:
    /// Moq answers a default interface member with an empty result of its own, which would skip the admission's writes.
    /// </summary>
    public static Mock<IUserAdministrationService> RunsTransactions(this Mock<IUserAdministrationService> mock)
    {
        ArgumentNullException.ThrowIfNull(mock);
        mock.Setup(service => service.InOneTransactionAsync(It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<bool>> work, CancellationToken cancellationToken) => work(cancellationToken));
        return mock;
    }
}
