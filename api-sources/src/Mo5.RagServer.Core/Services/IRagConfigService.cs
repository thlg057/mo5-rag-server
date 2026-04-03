using Mo5.RagServer.Core.Entities;

namespace Mo5.RagServer.Core.Services;

public interface IRagConfigService
{
    TagMappingConfig GetTagMapping();
    RerankingConfig GetReranking();
    QueryExpansionConfig GetQueryExpansion();
}