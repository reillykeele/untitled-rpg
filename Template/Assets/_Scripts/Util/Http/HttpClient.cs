using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Util.Http
{
    public class HttpClient : MonoBehaviour
    {
        public async Task<T> Get<T>(string uri)
        {
            var req = CreateRequest(uri, HttpMethod.Get);
            return await SendRequest<T>(req);
        }

        public async Task<T> Post<T>(string uri, object payload)
        {
            var req = CreateRequest(uri, HttpMethod.Post, payload);
            return await SendRequest<T>(req);
        }


        private UnityWebRequest CreateRequest(string uri, HttpMethod method, object data = null)
        {
            var req = new UnityWebRequest(uri, method.ToString());

            if (data != null)
            {
                var encodedData = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
                req.uploadHandler = new UploadHandlerRaw(encodedData);
            }

            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            return req;
        }

        private async Task<T> SendRequest<T>(UnityWebRequest req)
        {
            req.SendWebRequest();

            while (req.isDone == false) await Task.Delay(10);

            return JsonUtility.FromJson<T>(req.downloadHandler.text);
        }
    
    }
}
