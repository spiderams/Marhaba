namespace Taxi.Web.Api.Modules.Drivers;

public static class DriverDocumentSecurity
{
        public static bool HasValidSignature(
            ReadOnlySpan<byte> content,
            string contentType) => contentType switch
            {
                "image/jpeg" =>
                content.Length >= 3 &&
                content[0] == 0xFF &&
                content[1] == 0xD8 &&
                content[2] == 0xFF,

                "image/png" =>
                content.StartsWith(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),

                "application/pdf" =>
                content.StartsWith("%PDF-"u8),

                _ => false
            };
 }
