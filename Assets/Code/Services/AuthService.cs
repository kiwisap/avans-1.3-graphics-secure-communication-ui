using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Assets.Code.Services;
using System.Net.Http;
using Assets.Code.Models;

public class AuthService : AbstractService
{
    public async Task<ApiResult> RegisterAsync(string email, string password)
    {
        string url = $"{BaseUrl}/account/register";
        var requestData = new RegisterRequestDto { Email = email, Password = password };
        var json = JsonUtility.ToJson(requestData);

        using var request = CreateRequest(url, HttpMethod.Post, json);
        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            return ApiResult.Success();
        }
        else
        {
            return ApiResult.Fail($"Registration failed: {request.error} - {request.downloadHandler.text}");
        }
    }

    public async Task<ApiResult> LoginAsync(string email, string password)
    {
        string url = $"{BaseUrl}/account/login?useCookies=false&useSessionCookies=false";
        var requestData = new LoginRequestDto { Email = email, Password = password };
        var json = JsonUtility.ToJson(requestData);

        using var request = CreateRequest(url, HttpMethod.Post, json);
        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<AccessTokenResponseDto>(request.downloadHandler.text);
            Debug.Log($"Login successful. Token: {response.AccessToken}");
            PlayerPrefs.SetString("AccessToken", response.AccessToken);
            PlayerPrefs.Save();

            return ApiResult.Success();
        }
        else
        {
            return ApiResult.Fail($"Login failed: {request.error} - {request.downloadHandler.text}");
        }
    }
}
