using Microsoft.Win32.TaskScheduler;
using Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace WcfService1
{
    public class Service1 : IService1
    {
        private Connector connector;

        public Service1()
        {
            //if called each time??? ++++
            //connector = Connector.GetConnector();
        }
                
        public CityDictionary GetCityList()
        {
            Dictionary<string, string> dict = Connector.GetInstance().GetCityList();

            CityDictionary cityDictionary = new CityDictionary() { Dict = dict };
            return cityDictionary;
        }
        public CityDetailed GetCityDetailed(string id)
        {
            CityTemp cityTemp =  Connector.GetInstance().GetForecast(id, DateTime.Now.AddDays(1).Date);
            if (cityTemp == null) return null;

            CityDetailed detailed = new CityDetailed() 
            {
                ID = cityTemp.id,
                Date = cityTemp.Date,
                DayTemp = cityTemp.DayTemp,
                NightTemp = cityTemp.NightTemp,
                Name = cityTemp.Name
            };
            return detailed;
        }
        public string Ping()
        {
            return "OK";
        }

        public string DBState()
        {
            connector = Connector.GetInstance();
            return connector.IsConnected() ? "OK" : null;
        }

        public bool Update()
        {
            if (Connector.GetInstance().IsConnected())
            {
                GisMeteo parser = GisMeteo.GetInstance();
                if (parser.Ready)
                {
                    new System.Threading.Tasks.Task(() => parser.Update()).Start();
                    return true;
                }
                else return false;
            }
            return false;
        }

        public UpdateState GetUpdateState()
        {
            UpdateState updateState = new UpdateState()
            {
                Total = GisMeteo.GetInstance().Total,
                FinishedCity = GisMeteo.GetInstance().FinishedCity,
                Ready = GisMeteo.GetInstance().Ready
            };
            return updateState;
        }

        //set to interface
        private void setTask(int hour = 0, int minute = 0)
        {
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59) return;
            using (TaskService ts = new TaskService())
            {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Update GisMeteo";
                td.RegistrationInfo.Author = "Mirg0r0d";

                DateTime dateTime = DateTime.Parse($"{hour}:{minute}:00");

                // Create a trigger that will fire the task at this time every other day
                td.Triggers.Add(new DailyTrigger { StartBoundary = dateTime });

                var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
                while (directory != null && !directory.GetFiles("*.sln").Any())
                {
                    directory = directory.Parent;
                }
                directory = directory.Parent;
                directory = new DirectoryInfo(directory.FullName + "\\GisMeteoParser\\bin\\Debug\\");
                if (File.Exists(directory.FullName + "GisMeteoParser.exe"))
                {

                    td.Actions.Add(new ExecAction(directory.FullName + "GisMeteoParser.exe"));

                    ts.RootFolder.RegisterTaskDefinition("GisMeteoParserTask", td);
                }

            }
        }
        private void DeleteTask()
        {

        }

        public void SetTask(TaskParams taskParams)
        {
            using (TaskService ts = new TaskService())
            {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Update GisMeteo";
                td.RegistrationInfo.Author = "Mirg0r0d";

                // Create a trigger that will fire the task at this time every other day
                td.Triggers.Add(new DailyTrigger { StartBoundary = taskParams.time });

                string filePath = Path.Combine(new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).Parent.FullName, @"GisMeteoParser\bin\Debug\GisMeteoParser.exe");
                if (File.Exists(filePath))
                {

                    td.Actions.Add(new ExecAction(filePath));

                    ts.RootFolder.RegisterTaskDefinition(taskParams.Name, td);
                }

            }
        }

        public TaskParams GetTaskState(TaskParams taskParams)
        {
            var ts = new TaskService();
            var task = ts.RootFolder.GetTasks().Where(a => a.Name.ToLower() == taskParams.Name.ToLower()).FirstOrDefault();
            if (task == null) return null;
            else
            {
                return new TaskParams() { Name = task.Name, IsSet = true, time = task.NextRunTime };
            }

        }

        public void DeleteTask(TaskParams taskParams)
        {
            var ts = new TaskService();
            var task = ts.RootFolder.GetTasks().Where(a => a.Name.ToLower() == taskParams.Name.ToLower()).FirstOrDefault();
            if (task != null)
            {
                ts.RootFolder.DeleteTask(taskParams.Name);
            }
        }
    }
}
