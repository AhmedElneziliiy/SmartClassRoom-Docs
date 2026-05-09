namespace SmartClassRoom.Web.Services.Interfaces;

public class TermStatistics
{
    public int TermId { get; set; }
    public string TermName { get; set; } = string.Empty;
    public string TermCode { get; set; } = string.Empty;
    public int OfferingsCount { get; set; }
    public int ActiveOfferingsCount { get; set; }
    public int EnrolledStudentsCount { get; set; }
    public int ActiveEnrollmentsCount { get; set; }
}
