using UtilityExtensions;

namespace CmsData
{
    public interface ICurrentOrg
    {
        int? Id { get; set; }
        string NameFilter { get; set; }
        string SgFilter { get; set; }
        bool ShowHidden { get; set; }
    }
    public class CurrentOrg : ICurrentOrg
    {
        public int? Id { get; set; }
        public string NameFilter { get; set; }
        public string SgFilter { get; set; }
        public bool ShowHidden { get; set; }
    }

    public static class CurrentOrgExtensions
    {
        public static string First(this ICurrentOrg c)
        {
            if (!c.NameFilter.HasValue())
                return null;
            string first, last;
            Util.NameSplit(c.NameFilter, out first, out last);
            return first;
        }
        public static string Last(this ICurrentOrg c)
        {
            if (!c.NameFilter.HasValue())
                return null;
            string first, last;
            Util.NameSplit(c.NameFilter, out first, out last);
            return last;
        }
        public static void ClearCurrentOrg(this ICurrentOrg c)
        {
            c.SgFilter = null;
            c.NameFilter = null;
        }
    }
}