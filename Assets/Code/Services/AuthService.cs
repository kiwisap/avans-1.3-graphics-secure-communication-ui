using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using Assets.Code.Services;
using System.Net.Http;

public class AuthService : AbstractService
{
    public async Task<bool> RegisterAsync(string email, string password)
    {
        string url = $"{BaseUrl}/account/register";
        var requestData = new RegisterRequest { email = email, password = password };
        var json = JsonUtility.ToJson(requestData);

        using var request = CreateRequest(url, HttpMethod.Post, json);
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

    public async Task<bool> LoginAsync(string email, string password)
    {
        string url = $"{BaseUrl}/account/login?useCookies=false&useSessionCookies=false";
        var requestData = new LoginRequest { email = email, password = password };
        var json = JsonUtility.ToJson(requestData);

        using var request = CreateRequest(url, HttpMethod.Post, json);
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
