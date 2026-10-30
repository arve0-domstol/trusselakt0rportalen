namespace Saksbehandling.Api.Models;

/// <summary>Employee role, in ascending order of privilege.</summary>
public enum UserRole
{
    Saksbehandler = 0,
    SeniorSaksbehandler = 1,
    Teamleder = 2,
    Administrator = 3,
}

public enum CaseStatus
{
    Ny = 0,
    UnderBehandling = 1,
    VenterPaaKunde = 2,
    TilGodkjenning = 3,
    Godkjent = 4,
    Lukket = 5,
    Avvist = 6,
}

public enum CasePriority
{
    Lav = 0,
    Normal = 1,
    Hoy = 2,
    Kritisk = 3,
}
