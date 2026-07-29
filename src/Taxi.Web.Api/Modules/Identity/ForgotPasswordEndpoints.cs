using Taxi.Application.Identity.Auth.ForgotPassword;
using Taxi.SharedKernel.Messaging;
using Taxi.Web.Api.Endpoints;

namespace Taxi.Web.Api.Modules.Identity;

/// <summary>
/// Endpoints REST du module Identity (mot de passe oublié par OTP SMS).
/// </summary>
public sealed class ForgotPasswordEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/forgot-password", async (
            RequestPasswordResetCommand command,
            ICommandHandler<RequestPasswordResetCommand, bool> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("ForgotPassword")
        .WithTags(Tags.Identity)
        .WithSummary("Demander un code de réinitialisation")
        .WithDescription("Envoie un OTP SMS si le numéro correspond à un compte existant.");

        app.MapPost("/api/auth/reset-password", async (
            ResetPasswordCommand command,
            ICommandHandler<ResetPasswordCommand, bool> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.ToHttpResult();
        })
        .AllowAnonymous()
        .WithName("ResetPassword")
        .WithTags(Tags.Identity)
        .WithSummary("Réinitialiser le mot de passe")
        .WithDescription("Valide l'OTP SMS puis remplace le mot de passe du compte.");
    }
}