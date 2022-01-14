using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApiUrlFilter.Domain;

public class UrlFilter
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("urlkey")]
    public string UrlKey { get; set; }

    [Column("urlcontent")]
    public string UrlContent { get; set; }

    [Column("nonsecureaccess")]
    public bool NonSecureAccess { get; set; }

    [Column("secureaccess")]
    public bool SecureAccess { get; set; }

    [Column("allportsblocked")]
    public bool AllPortsBlocked { get; set; }

    [Column("domainblocked")]
    public bool DomainBlocked { get; set; }

    [Column("createddate")] 
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Column("createdby")]
    public string CreatedBy { get; set; }

    [Column("updateddate")]
    public DateTime UpdatedDate { get; set; }

    [Column("updatedby")]
    public string? UpdatedBy { get; set; }

    [Column("isactive")] 
    public bool IsActive { get; set; } = true;
}