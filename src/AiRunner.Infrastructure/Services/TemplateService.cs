using AiRunner.Domain.Interfaces;

namespace AiRunner.Infrastructure.Services;

public class TemplateService : ITemplateService
{
    private readonly IReadOnlyDictionary<string, string> _globalValues;

    public TemplateService(IConfigurationRepository configurationRepository)
    {
        var config = configurationRepository.GetConfigurationAsync().GetAwaiter().GetResult();
        _globalValues = new Dictionary<string, string>
        {
            ["root-folder"] = config.RootFolder
        };
    }

    public string Resolve(string template, IReadOnlyDictionary<string, string>? additionalValues = null)
    {
        var result = template;

        foreach (var (key, value) in _globalValues)
            result = result.Replace($"{{{{{key}}}}}", value);

        if (additionalValues is not null)
            foreach (var (key, value) in additionalValues)
                result = result.Replace($"{{{{{key}}}}}", value);

        return result;
    }
}
