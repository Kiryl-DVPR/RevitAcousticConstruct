﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using AcoustiCUtils;
using AcoustiCUtils.Library;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using ClassLibrary;
using Autodesk.Revit.DB;
using System.Security.Cryptography.X509Certificates;

namespace REST
{
    public static class Requests
    {
        private const string TestHost = "http://51.250.123.41:3005";
        private const string MainHost = "https://db.acoustic.ru:3005";

        public static async Task<HttpResponseMessage> PostRequest(string host,List<Constr> constr)
        {

            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            try
            {
                var json = JsonConvert.SerializeObject(constr);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(host, data);

                ClassLibrary.Logger.Logger_.Information($"Статус код:{response.StatusCode} \n " +
                                                        $"{response.RequestMessage.RequestUri}");

                return response;
            }
            catch (Exception x)
            {
                ClassLibrary.Logger.Logger_.Error($"{x.Message}");
                Console.WriteLine("Ошибка" + x.ToString());
            }
            finally
            {
                client.Dispose();
            }

            return null;

        }

        public static async Task<HttpResponseMessage> GetRequest(string host)
        {
            var client = new HttpClient();

            try
            {

                return await client.GetAsync(host);
            }
            catch (Exception x)
            {
                Console.WriteLine("Ошибка" + x.ToString());
            }
            finally
            {
                client.Dispose();
            }

            return null;

        }

        public static async Task<List<Product>> GetCalcProduct(List<Constr> constr)
        {

            var postresponse = await Requests.PostRequest($"{MainHost}/api/v1/calcIsolation/byProduct", constr);

            var jsonStringProduct = await postresponse.Content.ReadAsStringAsync(); // записываем содержимое файла в строковую переменную

            var response = JsonConvert.DeserializeObject<Response<Product>>(jsonStringProduct); // информацию из строковой переносим в список обьектов

            return response.data;
        }

        public static async Task<List<ListAG>> GetInfoConstr()
        {
            var postresponse = await Requests.GetRequest($"{MainHost}/api/v1/AllIsolationConstr");
            var jsonStringProduct = await postresponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<Response<ListAG>>(jsonStringProduct);
            return response?.data;
        }

        public static async Task<ResponseVersion> GetVersion()
        {
            HttpResponseMessage postresponse = await Requests.GetRequest($"{TestHost}/version");
            string jsonStringProduct = await postresponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseVersion>(jsonStringProduct);
            return response;
        }


    }
}
