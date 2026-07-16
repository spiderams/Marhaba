using Taxi.Application.Identity.Otp;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Identity;

/// <summary>
/// Endpoint de demande d'OTP SMS préalable à l'inscription.
/// </summary>
public sealed class RequestRegistrationOtpEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register/otp", async (
            RequestRegistrationOtpCommand command,
            ICommandHandler<RequestRegistrationOtpCommand, bool> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("RequestRegistrationOtp")
        .WithTags(Tags.Identity)
        .WithSummary("Envoyer un OTP SMS d'inscription")
        .WithDescription("Génère un code OTP SMS court pour vérifier le numéro de téléphone avant inscription.");
    }
}
