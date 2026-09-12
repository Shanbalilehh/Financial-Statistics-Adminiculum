namespace FinancialStatisticsAdminiculum.Core.Exceptions
{
    // Restrict this attribute so it can only be applied to classes or interfaces
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class RiskCommunityAttribute : Attribute
    {
        public string CommunityName { get; }

        public RiskCommunityAttribute(string communityName)
        {
            if (string.IsNullOrWhiteSpace(communityName))
                throw new ArgumentException("Community name cannot be null or empty.", nameof(communityName));

            CommunityName = communityName;
        }
    }
}