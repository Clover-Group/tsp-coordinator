namespace TspCoordinator.Data.TspApi;

using Actual = V3;

static class RequestValidator
{
    public static void Validate(Actual.Request request)
    {
        var idsAndSubunits = request.Patterns.Select(p => (p.Id, p.Subunit)).ToList();

        if (idsAndSubunits.Distinct().Count() != idsAndSubunits.Count)
        {
            throw new Exception("Duplicated id/subunit pairs found");
        }
    }
}