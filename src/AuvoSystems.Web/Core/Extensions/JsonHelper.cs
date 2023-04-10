using System.Runtime.Serialization.Json;
using System.Text;

namespace AuvoSystems.Web.Core.Extensions
{
    public static class JsonHelper
    {
        public static string ObjectToJSon<T>(this T obj)
        {
            try
            {
                DataContractJsonSerializer ser = new (typeof(T));
                MemoryStream ms = new();
                ser.WriteObject(ms, obj);
                string jsonString = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();
                return jsonString;
            }
            catch
            {
                throw;
            }
        }

        public static T JSonToObject<T>(this string jsonString)
        {
            try
            {
                DataContractJsonSerializer serializer = new(typeof(T));
                MemoryStream ms = new(Encoding.UTF8.GetBytes(jsonString));
                T obj = (T)serializer.ReadObject(ms);
                return obj;
            }
            catch
            {
                throw;
            }
        }
    }
}
