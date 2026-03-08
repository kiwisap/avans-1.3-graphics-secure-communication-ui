using Assets.Code.Models;
using Assets.Code.Utils;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Assets.Code.Services
{
    public class Object2DService : AbstractService
    {
        public async Task<ApiResult<Object2DDto[]>> GetObjectsByEnvironmentAsync(int environmentId)
        {
            string url = $"{BaseUrl}/api/objects/environment/{environmentId}";
            using var request = CreateRequest(url, HttpMethod.Get);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                return ApiResult<Object2DDto[]>.Fail($"GET objects mislukt: {request.error}");
            }

            try
            {
                var response = JsonUtils.Deserialize<Object2DListResponse>("{\"Items\":" + request.downloadHandler.text + "}");
                var items = response?.Items ?? Array.Empty<Object2DDto>();
                return ApiResult<Object2DDto[]>.Success(items);
            }
            catch (Exception e)
            {
                return ApiResult<Object2DDto[]>.Fail("GET objects: JSON parse error: " + e.Message);
            }
        }

        public async Task<ApiResult<Object2DDto>> CreateObjectAsync(Object2DDto body)
        {
            string url = $"{BaseUrl}/api/objects";
            var json = JsonUtils.Serialize(body);
            using var request = CreateRequest(url, HttpMethod.Post, json);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                return ApiResult<Object2DDto>.Fail($"POST object mislukt: {request.error}");
            }

            try
            {
                var created = JsonUtils.Deserialize<Object2DDto>(request.downloadHandler.text);
                return ApiResult<Object2DDto>.Success(created);
            }
            catch (Exception e)
            {
                return ApiResult<Object2DDto>.Fail("POST object: JSON parse error: " + e.Message);
            }
        }

        public async Task<ApiResult> DeleteObjectAsync(int objectId)
        {
            string url = $"{BaseUrl}/api/objects/{objectId}";
            using var request = CreateRequest(url, HttpMethod.Delete);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
                return ApiResult.Fail($"DELETE object mislukt: {request.error}");

            return ApiResult.Success();
        }
    }
}