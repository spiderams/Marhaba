using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Taxi.Infrastructure.Push;
using Xunit;

namespace Taxi.Application.Tests.Push;

public class FcmPushNotifierTests
{
    private static IOptions<FcmSettings> Options(string projectId = "taxi-dev")
        => Microsoft.Extensions.Options.Options.Create(new FcmSettings { ProjectId = projectId });

    [Fact]
    public async Task SendOfferAsync_envoie_le_message_au_bon_projet_avec_le_token()
    {
        var api = new Mock<IFcmApi>();
        var tokenProvider = new Mock<IFcmTokenProvider>();
        tokenProvider.Setup(t => t.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync("oauth-token-123");

        var sut = new FcmPushNotifier(api.Object, tokenProvider.Object, Options(), NullLogger<FcmPushNotifier>.Instance);

        await sut.SendOfferAsync("device-abc", rideId: 42, DateTime.UtcNow.AddSeconds(15), CancellationToken.None);

        api.Verify(a => a.SendAsync(
            "taxi-dev",
            It.Is<FcmRequest>(r =>
                r.Message.Token == "device-abc"
                && r.Message.Data["rideId"] == "42"),
            "Bearer oauth-token-123",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendOfferAsync_ne_propage_pas_les_exceptions_best_effort()
    {
        var api = new Mock<IFcmApi>();
        api.Setup(a => a.SendAsync(
                It.IsAny<string>(), It.IsAny<FcmRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("FCM indisponible"));
        var tokenProvider = new Mock<IFcmTokenProvider>();
        tokenProvider.Setup(t => t.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("tok");

        var sut = new FcmPushNotifier(api.Object, tokenProvider.Object, Options(), NullLogger<FcmPushNotifier>.Instance);

        // Ne doit pas lever : l'échec push est best-effort et ne casse pas le dispatch.
        Func<Task> act = () => sut.SendOfferAsync("device-abc", 42, DateTime.UtcNow.AddSeconds(15), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
