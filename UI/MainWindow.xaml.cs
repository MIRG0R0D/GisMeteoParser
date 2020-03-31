
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UI.ServiceReference1;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Service1Client client;
        private string TaskName = "GisMeteoParserTask";

        private BackgroundWorker updateBackgroundWorker ;
        private DateTime taskUpdateTime;
        public DateTime TaskUpdateTime
        {
            get
            {
                return taskUpdateTime;
            }
            set
            {
                taskUpdateTime = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            updateBackgroundWorker = new BackgroundWorker();
            updateBackgroundWorker.WorkerReportsProgress = true;
            updateBackgroundWorker.DoWork += BackgroundWorker1_DoWork;
            updateBackgroundWorker.ProgressChanged += BackgroundWorker1_ProgressChanged;
            updateBackgroundWorker.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
            client = new Service1Client();
            if (!string.IsNullOrEmpty(client.Ping()))
                setTextBosState(stateTxtWCF, "Online", Colors.Green);

            if (!string.IsNullOrEmpty(client.DBState()))
                setTextBosState(stateTxtDB, "Online", Colors.Green);

            if (isTaskSet())
            {
                scheduleChkBox.IsChecked = true;
                DateTime time = client.GetTaskState(new TaskParams() { Name = TaskName }).time;
                updateTimePcker.Value = time;
                setTextBosState(updateStateTxt, "Task set", Colors.Green);
            }
            else
            {
                scheduleChkBox.IsChecked = false;
                setTextBosState(updateStateTxt, "Task NOT set", Colors.Red);
            }

            updateCityList();
        }


        private void updateCityList()
        {
            cityListBox.Items.Clear();
            CityDictionary cityDictionary = client.GetCityList();
            if (cityDictionary != null) setTextBosState(stateTxtCityList, "Updated", Colors.Green);
            if (cityDictionary.Dict.Count==0) setTextBosState(stateTxtCityList, "Empty", Colors.Yellow);
            foreach (var city in cityDictionary.Dict)
            {
                ListBoxItem listBoxItem = new ListBoxItem()
                {
                    Content = city.Value,
                    Tag = city.Key
                };
                cityListBox.Items.Add(listBoxItem);
            }
        }

        private void setTextBosState (TextBlock box, string state, Color color)
        {
            box.Text = state;
            box.Foreground = new SolidColorBrush(color);

        }

        private void cityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem listBoxItem = (ListBoxItem) cityListBox.SelectedItem;
            setTextBosState(stateTxtForecast, "Updating", Colors.Yellow);
            CityDetailed detailed = client.GetCityDetailed(listBoxItem.Tag.ToString());
            if (detailed != null)
            {
                cityTxtBlock.Text = detailed.Name;
                dayTempTxtBlock.Text = detailed.DayTemp;
                nightTempTxtBlock.Text = detailed.NightTemp;
                dateTxtBlock.Text = detailed.Date.ToString("dd-MM-yyyy");
                setTextBosState(stateTxtForecast, "Updated", Colors.Green);
            }
            else setTextBosState(stateTxtForecast, "Failed", Colors.Red);
        }

        private void update_Click(object sender, RoutedEventArgs e)
        {
            updateFromDB();
        }

        private void updateFromDB()
        {
            updateProgress.Visibility = Visibility.Visible;
            updateProgress.Value = 0;
            updateTxt.Visibility = Visibility.Visible;
            updateTxt.Text = "Updating:";
            updateBackgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            updateProgress.Visibility = Visibility.Hidden;
            updateProgress.Value = 0;
            updateTxt.Visibility = Visibility.Hidden;
            updateCityList();
        }


        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            updateProgress.Value = e.ProgressPercentage;
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            client.Update();

            int counter = 0;
            while (true)
            {
                UpdateState state = client.GetUpdateState();
                if (state != null)
                {
                    if (state.Total != 0)
                    {
                        int percent = (int)(((double)state.FinishedCity / (double)state.Total) * 100.0);
                        updateBackgroundWorker.ReportProgress(percent);
                    }
                    if (state.Ready || (counter++) > 1000) return;
                    Thread.Sleep(100);
                }
            }
        }

        private void scheduleChkBox_Click(object sender, RoutedEventArgs e)
        {
            if (scheduleChkBox.IsChecked !=null && (bool)scheduleChkBox.IsChecked)
            {
                if (updateTimePcker.Value != null)
                {
                    DateTime time = ((DateTime)updateTimePcker.Value);
                    client.SetTask(new TaskParams() { Name = TaskName, time = time });
                }
            }
            else
            {
                if (isTaskSet())
                    client.DeleteTask(new TaskParams() { Name = TaskName});
            }

            if (isTaskSet())
                setTextBosState(updateStateTxt, "Task set", Colors.Green);
            else
                setTextBosState(updateStateTxt, "Task NOT set", Colors.Red);
        }

        private bool isTaskSet()
        {
            TaskParams taskParams = client.GetTaskState(new TaskParams() { Name = TaskName });
            return (taskParams != null && taskParams.IsSet);
        }
    }
}
