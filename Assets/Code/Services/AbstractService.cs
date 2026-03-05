using System.Net.Http;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Code.Services
{
	public abstract class AbstractService: MonoBehaviour
	{
        private static AbstractService Instance { get; set; }

        protected const string BaseUrl = "https://localhost:7222";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected UnityWebRequest CreateRequest(string url, HttpMethod method, string json = null)
        {
            var request = new UnityWebRequest(url, method.Method.ToUpper());

            if (json != null)
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            AddAuthorizationHeaderIfPresent(request);

            // Bypass SSL for localhost
            request.certificateHandler = new BypassCertificateHandler();

            return request;
        }

        private void AddAuthorizationHeaderIfPresent(UnityWebRequest req)
        {
            var authToken = PlayerPrefs.GetString("AccessToken", "");
            if (!string.IsNullOrWhiteSpace(authToken))
                req.SetRequestHeader("Authorization", authToken);
        }

        private class BypassCertificateHandler : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
    }
}