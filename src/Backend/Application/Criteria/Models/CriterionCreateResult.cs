namespace Application.Criteria.Models;

public sealed class CriterionCreateResult
{
    private CriterionCreateResult(CriterionCreateStatus status, CriterionResponse? criterion = null)
    {
        Status = status;
        Criterion = criterion;
    }

    public CriterionCreateStatus Status { get; }
    public CriterionResponse? Criterion { get; }

    public static CriterionCreateResult Success(CriterionResponse criterion) => new(CriterionCreateStatus.Success, criterion);
    public static CriterionCreateResult NotFound() => new(CriterionCreateStatus.NotFound);
    public static CriterionCreateResult Forbidden() => new(CriterionCreateStatus.Forbidden);
}

public enum CriterionCreateStatus
{
    Success,
    NotFound,
    Forbidden
}
