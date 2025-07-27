namespace ECommerce.Infrastructure.Messaging;

/// <summary>
/// Interface for message serialization and deserialization
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Serializes an object to byte array
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="obj">The object to serialize</param>
    /// <returns>Serialized byte array</returns>
    byte[] Serialize<T>(T obj) where T : class;

    /// <summary>
    /// Deserializes a byte array to an object
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="data">The byte array to deserialize</param>
    /// <returns>Deserialized object</returns>
    T Deserialize<T>(byte[] data) where T : class;

    /// <summary>
    /// Deserializes a byte array to an object of the specified type
    /// </summary>
    /// <param name="data">The byte array to deserialize</param>
    /// <param name="type">The type to deserialize to</param>
    /// <returns>Deserialized object</returns>
    object Deserialize(byte[] data, Type type);
}