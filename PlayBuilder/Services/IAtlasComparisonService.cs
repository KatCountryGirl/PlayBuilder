using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface IAtlasComparisonService
{
    AtlasComparisonReport Compare(ArchiveScanResult scan, CollectionRuleOptions options);
}
