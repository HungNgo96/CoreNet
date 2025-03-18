using System.ComponentModel.DataAnnotations;
using Domain.Core.SharedKernel;

namespace Domain.Core.AppSettings;

public sealed class ConnectionOptions : IAppOptions
{
    static string IAppOptions.ConfigSectionPath => "ConnectionStrings";

    [Required]
    public string? ReadSqlServer { get; set; }

    [Required]
    public string? WriteSqlServer { get; set; }

    //[Required]
    public string? NoSqlConnection { get; set; }

    //[Required]
    public string? CacheConnection { get; set; }

    public bool CacheConnectionInMemory() =>
        CacheConnection!.Equals("InMemory", StringComparison.InvariantCultureIgnoreCase);
}
