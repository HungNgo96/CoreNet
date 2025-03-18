namespace Domain.Core.Abstractions
{
    /// <summary>
    /// Represents the marker interface for auditable entities.
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Gets the created on date and time in UTC format.
        /// </summary>
        long CreatedOnUtc { get; }

        /// <summary>
        /// Gets the user who created this entity.
        /// </summary>
        string CreatedBy { get; }

        /// <summary>
        /// Gets the modified on date and time in UTC format.
        /// </summary>
        long? ModifiedOnUtc { get; }

        /// <summary>
        /// Gets the user who last modified this entity.
        /// </summary>
        string? ModifiedBy { get; }
    }
}
