using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Shared;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.Shared;

public class EventBusMessagePolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        var baseType = typeof(EventBusMessage);
        if (jsonTypeInfo.Type != baseType)
        {
            return jsonTypeInfo;
        }

        var derivedTypes = baseType.Assembly.DefinedTypes
            .Where(x => x is { IsClass: true, IsAbstract: false } && x.BaseType == baseType)
            .Select(x => new JsonDerivedType(x, x.Name)).ToList();

        var polymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = baseType.Name,
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };

        foreach (var derivedType in derivedTypes)
        {
            polymorphismOptions.DerivedTypes.Add(derivedType);
        }

        jsonTypeInfo.PolymorphismOptions = polymorphismOptions;

        return jsonTypeInfo;
    }
}