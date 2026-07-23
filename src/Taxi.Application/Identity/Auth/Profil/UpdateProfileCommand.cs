using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Identity.Auth.Profile;

/// <summary>
/// Met à jour les informations éditables depuis l'écran profil.
/// </summary>
public sealed record UpdateProfileCommand(string UserId, string FullName) : ICommand<UserInfo>;