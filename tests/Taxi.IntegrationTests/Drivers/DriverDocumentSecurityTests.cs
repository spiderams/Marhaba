using FluentAssertions;
using Taxi.Web.Api.Modules.Drivers;
using Xunit;

namespace Taxi.IntegrationTests.Drivers;

public sealed class DriverDocumentSecurityTests
{
    [Theory]
    [InlineData("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0x00 })]
    [InlineData("image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })]
    [InlineData("application/pdf", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D })]
    public void Known_file_signatures_should_be_accepted(string contentType, byte[] content)
    {
        DriverDocumentSecurity.HasValidSignature(content, contentType).Should().BeTrue();
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("application/pdf")]
    public void Spoofed_content_should_be_rejected(string contentType)
    {
        DriverDocumentSecurity.HasValidSignature("not-the-announced-format"u8, contentType)
            .Should().BeFalse();
    }
}