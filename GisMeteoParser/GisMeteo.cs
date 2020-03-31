using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Parser
{

    public class GisMeteo
    {

        public int Total { get; set; }
        public int FinishedCity { get; set; }
        public bool Ready { get; set; } = true;

        private static readonly GisMeteo instance = new GisMeteo();
        public static GisMeteo GetInstance() => instance;
        private GisMeteo()
        {
            Notify += GisMeteo_Notify;
        }

        private void GisMeteo_Notify(int done)
        {
            FinishedCity = done;
            //if (FinishedCity == Total - 1) Ready = true;
        }

        public delegate void ParserHandler(int done);
        public event ParserHandler Notify;
        public static void Main(string[] args)
        {
            //russian console output
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("GisMeteoParser start");
            
            Console.Write("Getting cities list: ");
            Task<List<CityTemp>> getCitiesTask = new Task<List<CityTemp>>(() => ParserEngine.getCities());
            getCitiesTask.Start();
            List<CityTemp> list = getCitiesTask.Result;
            Console.WriteLine("found {0} cities", list.Count);
            Console.WriteLine("Getting detailed forecast for tommorow:");

            Task[] tasks = list.Select(x => new Task(() => ParserEngine.FillCity(x))).Where(x => { x.Start(); return true; }).ToArray();

            Task.WaitAll(tasks);

            writeToDB(list);
        }


        public bool Update()
        {
            Ready = false;
            Total = 0;
            FinishedCity = 0;
            Task<List<CityTemp>> getCitiesTask = new Task<List<CityTemp>>(() => ParserEngine.getCities());
            getCitiesTask.Start();
            List<CityTemp> list = getCitiesTask.Result;
            Total = list.Count;
            Task[] tasks = list.Select(x => new Task(() => ParserEngine.FillCity(x, Notify))).ToArray();
            tasks.ToList().ForEach(x => x.Start());
            Task.WaitAll(tasks);
            writeToDB(list);
            Ready = true;
            return true;
        }

        private static void writeToDB(List<CityTemp> list)
        {
            Console.WriteLine("Saving to DB");
            foreach (CityTemp city in list)
            {
                Connector.GetInstance().AddForecast(city.Date, city.DayTemp, city.NightTemp, city.Name, city.id);
            }
            Console.WriteLine("Saved");
        }
    }
}
