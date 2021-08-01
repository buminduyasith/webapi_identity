using webapi_identity.Repositorys;
using System;
namespace webapi_identity.Services
{
    public class WeatherService : IRepoWeather
    {
        public void UserInfo()
        {
            Console.WriteLine("done--------------------------------");
        }
    }
}