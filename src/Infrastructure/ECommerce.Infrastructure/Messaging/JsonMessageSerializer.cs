using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ECommerce.Infrastructure.Messaging;

/// <summary>
/// JSON-based message serializer implementation
/// </summary>
public class JsonMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options;

    public JsonMessageSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <inheritdoc />
    public byte[] Serialize<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);
        
        var json = JsonSerializer.Serialize(obj, _options);
        return Encoding.UTF8.GetBytes(json);
    }

    /// <inheritdoc />
    public T Deserialize<T>(byte[] data) where T : class
    {
        ArgumentNullException.ThrowIfNull(data);
        
        var json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<T>(json, _options) 
            ?? throw new InvalidOperationException($"Failed to deserialize message to type {typeof(T).Name}");
    }

    /// <inheritdoc />
    public object Deserialize(byte[] data, Type type)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(type);
        
        var json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize(json, type, _options) 
            ?? throw new InvalidOperationException($"Failed to deserialize message to type {type.Name}");
    }
}