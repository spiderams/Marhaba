using Taxi.Application.Drivers;
using Taxi.SharedKernel.Messaging;

namespace Taxi.Application.Administration.Drivers;

public sealed record SuspendDriverCommand(int DriverId) : ICommand<DriverDto>;