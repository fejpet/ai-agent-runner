namespace AiRunner.Domain.Interfaces;

public interface ITemplateService
{
    string Resolve(string template, IReadOnlyDictionary<string, string>? additionalValues = null);
}
