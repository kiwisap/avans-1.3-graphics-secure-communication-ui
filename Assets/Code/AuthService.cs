using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;
using System;

public class AuthService : MonoBehaviour
{
    public static AuthService Instance { get; private set; }

    private const string BaseUrl = "https://localhost:7222";

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

    public async Task<bool> Register(string email, string password)
    {
        string url = $"{BaseUrl}/account/register";
        var requestData = new RegisterRequest { email = email, password = password };
        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = CreateRequest(url, "POST", json))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Registration successful");
                return true;
            }
            else
            {
                Debug.LogError($"Registration failed: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    public async Task<bool> Login(string email, string password)
    {
        string url = $"{BaseUrl}/account/login?useCookies=false&useSessionCookies=false";
        var requestData = new LoginRequest { email = email, password = password };
        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = CreateRequest(url, "POST", json))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<AccessTokenResponse>(request.downloadHandler.text);
                Debug.Log($"Login successful. Token: {response.accessToken}");
                PlayerPrefs.SetString("AccessToken", response.accessToken);
                PlayerPrefs.Save();
                return true;
            }
            else
            {
                Debug.LogError($"Login failed: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    private UnityWebRequest CreateRequest(string url, string method, string json)
    {
        var request = new UnityWebRequest(url, method);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Bypass SSL for localhost
        request.certificateHandler = new BypassCertificateHandler();
        
        return request;
    }

    private class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    [Serializable]
    private class RegisterRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    private class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    private class AccessTokenResponse
    {
        public string tokenType;
        public string accessToken;
        public long expiresIn;
        public string refreshToken;
    }
}
