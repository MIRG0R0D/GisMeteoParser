using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WcfService1
{
    [ServiceContract]
    public interface IService1
    {
        //[OperationContract]
        //CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        string Ping();
        [OperationContract]
        string DBState();


        [OperationContract]
        CityDictionary GetCityList();

        [OperationContract]
        CityDetailed GetCityDetailed(string id);

        [OperationContract]
        bool Update();

        [OperationContract]
        UpdateState GetUpdateState();

        [OperationContract]
        void SetTask(TaskParams taskParams);
        [OperationContract]
        TaskParams GetTaskState(TaskParams taskParams);
        [OperationContract]
        void DeleteTask(TaskParams taskParams);
    }

    [DataContract]
    public class TaskParams
    {
        [DataMember]
        public DateTime time { get; set; }
        [DataMember]
        public bool IsSet { get; set; }
        [DataMember]
        public string Name { get; set; }

    }
    [DataContract]
    public class CityDictionary
    {
        [DataMember]
        public Dictionary<string, string> Dict { get; set; }
    }

    [DataContract]
    public class UpdateState
    {
        [DataMember]
        public int Total { get; set; }
        [DataMember]
        public int FinishedCity { get; set; }
        [DataMember]
        public bool Ready { get; set; } 
    }

    [DataContract]
    public class CityDetailed
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string DayTemp { get; set; }
        [DataMember]
        public string NightTemp { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public string ID{ get; set; }
    }
    
}
