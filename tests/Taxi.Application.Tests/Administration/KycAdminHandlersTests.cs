using FluentAssertions;
using Moq;
using Taxi.Application.Abstractions;
using Taxi.Application.Administration.Drivers;
using Taxi.Application.Drivers;
using Taxi.Domain.Drivers;
using Xunit;

namespace Taxi.Application.Tests.Administration;

/// <summary>
/// Tests des handlers d'administration KYC (approbation / suspension / rejet d'un chauffeur) :
/// succès, chauffeur introuvable et transition de statut invalide.
/// </summary>
public class KycAdminHandlersTests
{
    private static Mock<IRepository<Driver>> RepoReturning(Driver? driver)
    {
        var repo = new Mock<IRepository<Driver>>();
        repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<DriverByIdSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);
        return repo;
    }

    private static Driver NewPendingDriver() => Driver.Create("u-1", "LIC-001", "DJ-1234", "Taxi");
    private static void AddRequiredDocuments(Driver driver)
    {
        driver.SetDocument(DriverDocumentType.License, "drivers/1/license.pdf");
        driver.SetDocument(DriverDocumentType.VehicleRegistration, "drivers/1/registration.pdf");
        driver.SetDocument(DriverDocumentType.Identity, "drivers/1/identity.pdf");
    }

    // --- Approve ---

    [Fact]
    public async Task Approve_should_approve_a_pending_driver()
    {
        var driver = NewPendingDriver();
        AddRequiredDocuments(driver);
        var repo = RepoReturning(driver);
        var handler = new ApproveDriverCommandHandler(repo.Object);

        var result = await handler.Handle(new ApproveDriverCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        driver.Status.Should().Be(DriverStatus.Approved);
        repo.Verify(r => r.UpdateAsync(driver, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Approve_should_fail_when_a_required_document_is_missing()
    {
        var driver = NewPendingDriver();
        driver.SetDocument(DriverDocumentType.License, "drivers/1/license.pdf");
        driver.SetDocument(DriverDocumentType.Identity, "drivers/1/identity.pdf");
        var repo = RepoReturning(driver);
        var handler = new ApproveDriverCommandHandler(repo.Object);

        var result = await handler.Handle(new ApproveDriverCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DriverErrors.MissingRequiredDocuments);
        driver.Status.Should().Be(DriverStatus.PendingApproval);
        repo.Verify(r => r.UpdateAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Approve_should_fail_when_driver_not_found()
    {
        var repo = RepoReturning(null);
        var handler = new ApproveDriverCommandHandler(repo.Object);

        var result = await handler.Handle(new ApproveDriverCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Driver.NotFound");
        repo.Verify(r => r.UpdateAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Approve_should_propagate_domain_error_on_invalid_transition()
    {
        var driver = NewPendingDriver();
        driver.Approve(); // déjà Approved → un second Approve doit échouer
        var repo = RepoReturning(driver);
        var handler = new ApproveDriverCommandHandler(repo.Object);

        var result = await handler.Handle(new ApproveDriverCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DriverErrors.InvalidStatusTransition);
    }

    // --- Suspend ---

    [Fact]
    public async Task Suspend_should_suspend_an_approved_driver()
    {
        var driver = NewPendingDriver();
        driver.Approve();
        var repo = RepoReturning(driver);
        var handler = new SuspendDriverCommandHandler(repo.Object);

        var result = await handler.Handle(new SuspendDriverCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        driver.Status.Should().Be(DriverStatus.Suspended);
        repo.Verify(r => r.UpdateAsync(driver, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Suspend_should_fail_when_driver_not_found()
    {
        var repo = RepoReturning(null);
        var handler = new SuspendDriverCommandHandler(repo.Object);

        var result = await handler.Handle(new SuspendDriverCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Driver.NotFound");
    }

    [Fact]
    public async Task Suspend_should_fail_when_driver_not_approved()
    {
        var driver = NewPendingDriver(); // encore PendingApproval
        var repo = RepoReturning(driver);
        var handler = new SuspendDriverCommandHandler(repo.Object);

        var result = await handler.Handle(new SuspendDriverCommand(1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DriverErrors.InvalidStatusTransition);
    }

    // --- Reject ---

    [Fact]
    public async Task Reject_should_reject_a_pending_driver()
    {
        var driver = NewPendingDriver();
        var repo = RepoReturning(driver);
        var handler = new RejectDriverCommandHandler(repo.Object);

        var result = await handler.Handle(new RejectDriverCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        driver.Status.Should().Be(DriverStatus.Rejected);
        repo.Verify(r => r.UpdateAsync(driver, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reject_should_fail_when_driver_not_found()
    {
        var repo = RepoReturning(null);
        var handler = new RejectDriverCommandHandler(repo.Object);

        var result = await handler.Handle(new RejectDriverCommand(99), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Driver.NotFound");
    }
}
