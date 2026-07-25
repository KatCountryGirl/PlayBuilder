using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface ICollectionRuleService
{
    CollectionRulePreview BuildPreview(ArchiveScanResult scan, CollectionRuleOptions options);
}
