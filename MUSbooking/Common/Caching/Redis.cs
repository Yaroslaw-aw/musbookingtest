using Microsoft.Extensions.Caching.Distributed;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Text;

namespace MUSbooking.Common.Caching
{
    public class Redis(IDistributedCache cache)
    {
        public async Task<T?> GetDataAsync<T>(string key)
        {
            string? value = await cache.GetStringAsync(key);

            if (string.IsNullOrWhiteSpace(value))
                return default;

            return await Task.Run(() => JsonSerializer.Deserialize<T>(value));
        }

        public async Task SetDataAsync<T>(string key, T? value, int TTL)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true,
            };

            // Создаем поток MemoryStram для сериализации объекта
            using (MemoryStream stream = new MemoryStream())
            {
                // Сериализуем объект в поток
                await JsonSerializer.SerializeAsync(stream, value, typeof(T), options);

                // Перемещаем указатель потока в начало
                stream.Position = 0;

                // Чтение данных из потока частями
                using (StreamReader reader = new StreamReader(stream))
                {
                    // Создаем буфер для чтения данных
                    char[] buffer = new char[1024];
                    int bytesRead;
                    StringBuilder sb = new StringBuilder();

                    // Читаем данные из потока и записываем их в строку
                    while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        sb.Append(new string(buffer, 0, bytesRead));
                    }
                    string jsonString = sb.ToString();

                    // Создаем опции для записи в кэш
                    DistributedCacheEntryOptions cacheEntryOptions = new DistributedCacheEntryOptions();
                    cacheEntryOptions.AbsoluteExpiration = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(TTL);

                    // Сохраняем данные в кэш
                    await cache.SetStringAsync(key, jsonString, cacheEntryOptions);
                }
            }
        }




        public bool TryGetValue<T>(string key, out T? value)
        {
            T? data = GetData<T>(key);
            if (data is null)
            {
                value = default;
                return false;
            }
            value = data;
            return true;
        }


        public T? GetData<T>(string key)
        {
            string? value = cache.GetString(key);

            if (!string.IsNullOrEmpty(value))
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }

        public void SetData<T>(string key, T? value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true,
            };

            string jsonString = JsonSerializer.Serialize(value, options);
            cache.SetString(key, jsonString);
        }

    }
}
