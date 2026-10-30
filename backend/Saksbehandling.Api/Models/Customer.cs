namespace Saksbehandling.Api.Models;

/// <summary>
/// Kunde. The external party a case concerns. All data is synthetic
/// training data - never a real person.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Synthetic identifier shaped like a Norwegian "fødselsnummer"
    /// (ddmmyy-nnnnn) for realism. Never a real national ID.
    /// </summary>
    public string SyntheticNationalId { get; set; } = string.Empty;

    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
