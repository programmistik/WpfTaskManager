using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfTaskManager
{
    public class TaskManagerViewModel : ViewModelBase
    {

        private ObservableCollection<ProcessItem> processCollection;
        public ObservableCollection<ProcessItem> ProcessCollection { get => processCollection; set => Set(ref processCollection, value); }

        private ProcessItem selItem;
        public ProcessItem SelItem { get => selItem; set => Set(ref selItem, value); }

        private string startProcess;
        public string StartProcess { get => startProcess; set => Set(ref startProcess, value); }

        private Timer tm;
        private object _itemsLock = new object();

        public TaskManagerViewModel()
        {
            
            ProcessCollection = new ObservableCollection<ProcessItem>();
           // BindingOperations.EnableCollectionSynchronization(ProcessCollection, _itemsLock);

            foreach (var item in Process.GetProcesses())
            {
                try
                {
                    var newItem = new ProcessItem();
                    newItem.Pid = item.Id;
                    newItem.Name = item.ProcessName;
                    // newItem.UserName = GetProcessOwner(item.Id);
                    // newItem.Cpu = GetUssage(item.Id.ToString());
                    try
                    {
                        Icon ico = Icon.ExtractAssociatedIcon(item.MainModule.FileName);

                        using (Bitmap bmp = ico.ToBitmap())
                        {
                            var stream = new MemoryStream();
                            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            newItem.Img = BitmapFrame.Create(stream);
                        }
                    }
                    catch (Exception) { }
                    ProcessCollection.Add(newItem);
                   
                }
                catch (Exception) { }
                //  
                //Thread thread = new Thread(() =>
                //{
                    tm = new Timer(LoadProcesses,null,0,5000);
                    //tm.Tick += TimerOnTick;
                    //tm.Interval = new TimeSpan(0, 0, 8);
                   // tm.Start();
                //});

            }
        }

        private  void TimerOnTick(object sender)
        {
          //  ProcessCollection.Clear();
            // Task.Run(() => LoadProcesses());
           // Task.Factory.StartNew(() => LoadProcesses());
        }

        public void LoadProcesses(object sender)
        {
            //int id;
            //if (SelItem != null)
            //{
            //    id = SelItem.Pid;
            //}
                var newColl = new ObservableCollection<ProcessItem>();
            foreach (var item in Process.GetProcesses())
            {
                try
                {
                    var newItem = new ProcessItem();
                    newItem.Pid = item.Id;
                    newItem.Name = item.ProcessName;
                    //  newItem.UserName = GetProcessOwner(item.Id);
                    // newItem.Cpu = GetUssage(item.Id.ToString());
                    try
                    {
                        Icon ico = Icon.ExtractAssociatedIcon(item.MainModule.FileName);

                        using (Bitmap bmp = ico.ToBitmap())
                        {
                            var stream = new MemoryStream();
                            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            newItem.Img = BitmapFrame.Create(stream);
                        }
                    }
                    catch (Exception) { }
                    if (!newColl.Where(i => i.Pid == newItem.Pid).Any())
                        newColl.Add(newItem);

                    //lock (_itemsLock)
                    //{
                    //    // Once locked, you can manipulate the collection safely from another thread
                    //    // if (!ProcessCollection.Where(i => i.Pid == newItem.Pid).Any())
                    //    ProcessCollection.Add(newItem);
                    //}
                }
                catch (Exception) { }
            }
            
            Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                //ProcessCollection = newColl;

            });

            //if (SelItem != null)
            //{
            //    //var pname = SelItem.Name;
            //    //if (ProcessCollection.Where(n => n.Name == pname) != null)
            //        SelItem = ProcessCollection.Where(n => n.Pid == id).FirstOrDefault();

            //}
        }

        public string GetProcessOwner(int processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    //  DOMAIN\user
                    return argList[1] + "\\" + argList[0];
                }
            }

            return "NO OWNER";
        }
        public static decimal GetUssage(string pid)
        {
            //get the process
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ProcessID = " +pid);
            decimal PercentProcessorTime = 0;
            foreach (ManagementObject queryObj in searcher.Get())
            {
                DateTime firstSample, secondSample;

                //populate the process info
                firstSample = DateTime.Now;
                queryObj.Get();
                //get cpu usage
                ulong u_oldCPU = (ulong)queryObj.Properties["UserModeTime"].Value
                + (ulong)queryObj.Properties["KernelModeTime"].Value;
                //sleep to create interval
                System.Threading.Thread.Sleep(1000);
                //refresh object
                secondSample = DateTime.Now;
                queryObj.Get();
                //get new usage
                ulong u_newCPU = (ulong)queryObj.Properties["UserModeTime"].Value
                + (ulong)queryObj.Properties["KernelModeTime"].Value;

                decimal msPassed = (decimal)((secondSample - firstSample).TotalMilliseconds);

                //formula to get CPU ussage
                if (u_newCPU > u_oldCPU)
                    PercentProcessorTime = (decimal)((u_newCPU - u_oldCPU) / (msPassed * 100 * Environment.ProcessorCount));

            }
            return PercentProcessorTime;
        }
        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get => addCommand ?? (addCommand = new RelayCommand(
                () =>
                {
                    if (!String.IsNullOrEmpty(StartProcess))
                    {
                        try
                        {
                            Process.Start(StartProcess);
                            MessageBox.Show("Started");
                            StartProcess = "";
                            ProcessCollection.Clear();
                            LoadProcesses(null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                }
            ));
        }
        private RelayCommand endCommand;
        public RelayCommand EndCommand
        {
            get => endCommand ?? (endCommand = new RelayCommand(
                () =>
                {
                    var qw = Process.GetProcessById(SelItem.Pid);
                    if (qw != null)
                    {
                        qw.Kill();
                        SelItem = null;
                        MessageBox.Show("Killed☠");
                        ProcessCollection.Clear();
                        LoadProcesses(null);
                    }
                    
                }
            ));
        }

    }
}
