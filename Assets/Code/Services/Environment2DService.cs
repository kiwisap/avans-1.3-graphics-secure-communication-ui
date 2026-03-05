using Assets.Code.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Code.Services
{
    public class Environment2DService : AbstractService
    {
        public async Task<ApiResult<Environment2DDto[]>> GetEnvironmentsAsync()
        {
            string url = $"{BaseUrl}/api/environments";
            using var request = CreateRequest(url, HttpMethod.Get);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
                return ApiResult<Environment2DDto[]>.Fail($"GET environments mislukt: {request.error}");

            string json = request.downloadHandler.text;

            try
            {
                var response = JsonUtility.FromJson<Environment2DListResponse>("{\"items\":" + json + "}");
                var items = response?.items ?? Array.Empty<Environment2DDto>();
                return ApiResult<Environment2DDto[]>.Success(items);
            }
            catch (Exception e)
            {
                return ApiResult<Environment2DDto[]>.Fail("GET environments: JSON parse error: " + e.Message);
            }
        }

        public async Task<ApiResult> CreateEnvironmentAsync(Environment2DDto body)
        {
            string url = $"{BaseUrl}/api/environments";
            var json = JsonUtility.ToJson(body);
            var request = CreateRequest(url, HttpMethod.Post, json);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
                return ApiResult.Fail($"POST environment mislukt: {request.error}");

            return ApiResult.Success();
        }

        public async Task<ApiResult> DeleteEnvironmentAsync(int environmentId)
        {
            string url = $"{BaseUrl}/api/environments/{environmentId}";
            using var request = CreateRequest(url, HttpMethod.Delete);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
                return ApiResult.Fail($"DELETE environment mislukt: {request.error}");

            return ApiResult.Success();
        }
    }
}
