namespace VersionControl.Infrastructure;

internal interface ISerializer
{
    string Serialize(object obj);

    T Deserialize<T>(string json);
}

internal class Serializer : ISerializer
{
    public string Serialize(object obj)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
    }

    public T Deserialize<T>(string json)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json)!;
    }
}
