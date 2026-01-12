using System.Text;
using SimpleUrlShortener.Analytics.Domain;

namespace SimpleUrlShortener.Analytics.Infrastructure;

public class CsvConverter : IConverter<IEnumerable<Url>>
{
    public string Convert(IEnumerable<Url> urls)
    {
        var csv = new StringBuilder();
        
        // Заголовок CSV
        csv.AppendLine("Original,Code,CreationMoments,ClickMoments");
        
        // Данные
        foreach (var url in urls)
        {
            var creationMoments = string.Join(";", url.CreationMoments.Select(m => m.ToString("o")));
            var clickMoments = string.Join(";", url.ClickMoments.Select(m => m.ToString("o")));
            
            csv.AppendLine($"{Escape(url.Original)},{Escape(url.Code)},{Escape(creationMoments)},{Escape(clickMoments)}");
        }
        
        return csv.ToString();
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            value = $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}