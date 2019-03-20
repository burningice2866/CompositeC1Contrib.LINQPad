using System;
using System.Collections.Generic;
using System.Net.Http;

using Newtonsoft.Json;

namespace C1Contrib.LINQPad
{
    public class C1DataRetriever
    {
        private readonly HttpClient _client = new HttpClient();

        private readonly Uri _uri;
        private readonly string _userName;
        private readonly string _password;

        public C1DataRetriever(Uri uri, string userName, string password)
        {
            _uri = uri;
            _userName = userName;
            _password = password;
        }

        public List<EntitySchema> ListTypes()
        {
            var uri = new Uri(_uri, "/api/linqpad/types");
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("UserName", _userName),
                new KeyValuePair<string, string>("Password", _password)
            };

            var response = _client.PostAsync(uri, new FormUrlEncodedContent(postData)).Result;

            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().Result;
            var list = JsonConvert.DeserializeObject<List<EntitySchema>>(json);

            foreach (var entity in list)
            {
                foreach (var prop in entity.Properties)
                {
                    var type = Type.GetType(prop.Type);

                    prop.Type = GetTypeRepresentation(type, false);
                }
            }

            return list;
        }

        public List<T> Fetch<T>(Guid typeId) where T : class, new()
        {
            var uri = new Uri(_uri, "/api/linqpad/fetch");
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("UserName", _userName),
                new KeyValuePair<string, string>("Password", _password),
                new KeyValuePair<string, string>("TypeId", typeId.ToString())
            };

            var response = _client.PostAsync(uri, new FormUrlEncodedContent(postData)).Result;

            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        private static string GetTypeRepresentation(Type type, bool isNull)
        {
            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType != null)
            {
                return GetTypeRepresentation(underlyingNullableType, true);
            }

            if (type.IsValueType || type == typeof(string))
            {
                var name = type.Name;
                if (isNull)
                {
                    name += "?";
                }

                return name;
            }

            return type.FullName;
        }
    }
}
