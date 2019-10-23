using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HalmaEditor.Tools
{
    public class SettingsUpdater
    {
        public string UpdateSettings(string old, string template)
        {
            using var oldRoot = JsonDocument.Parse(old);
            using var TemplateRoot = JsonDocument.Parse(template);
            object newRoot = UpdateObj(oldRoot.RootElement, TemplateRoot.RootElement);
            return JsonSerializer.Serialize(newRoot, new JsonSerializerOptions { WriteIndented = true });
        }

        public object UpdateObj(JsonElement o, JsonElement t)
        {
            switch (t.ValueKind)
            {
                case JsonValueKind.True:
                case JsonValueKind.False:
                    if(o.ValueKind == JsonValueKind.True || o.ValueKind == JsonValueKind.False)
                    {
                        return o.GetBoolean();
                    }
                    return t.GetBoolean();
                case JsonValueKind.Number:
                    if (o.ValueKind == JsonValueKind.Number)
                    {
                        return o.GetDouble();
                    }
                    return t.GetDouble();
                case JsonValueKind.String:
                    if (o.ValueKind == JsonValueKind.String)
                    {
                        return o.GetString();
                    }
                    return t.GetString();
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Object:
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    if (o.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var item in t.EnumerateObject())
                        {
                            try
                            {
                                var oprop = o.GetProperty(item.Name);
                                dic.Add(item.Name, UpdateObj(oprop, item.Value));
                            }
                            catch
                            {
                                dic.Add(item.Name, item.Value);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in t.EnumerateObject())
                        {
                            dic.Add(item.Name, UpdateObj(default, item.Value));
                        }
                    }
                    return dic;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
