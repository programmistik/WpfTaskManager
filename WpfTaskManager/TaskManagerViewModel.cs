using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public TaskManagerViewModel()
        {
            
            ProcessCollection = new ObservableCollection<ProcessItem>();

            foreach (var item in Process.GetProcesses())
            {
                try
                {
                    var newItem = new ProcessItem();
                    newItem.Pid = item.Id;
                    newItem.Name = item.ProcessName;
                   
                    ProcessCollection.Add(newItem);

                }
                catch (Exception) { }

                tm = new Timer(LoadProcesses, null, 0, 5000);

            }
        }

   
        public void LoadProcesses(object sender)
        {
            var newColl = new ObservableCollection<ProcessItem>();

            foreach (var item in Process.GetProcesses())
            {
                try
                {
                    if (ProcessCollection.Where(i => i.Pid == item.Id).Any())
                    {
                        var oldItem = ProcessCollection.Where(i => i.Pid == item.Id).Single();
                        newColl.Add(oldItem);
                    }
                    else
                    {
                        var newItem = new ProcessItem();
                        newItem.Pid = item.Id;
                        newItem.Name = item.ProcessName;
                        newColl.Add(newItem);
                    }
                }
                catch (Exception) { }
            }
            
            Application.Current.Dispatcher.Invoke( () =>
            {
                ProcessCollection = newColl;
            });

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
                           // MessageBox.Show("Started");
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
                    if (SelItem == null)
                        MessageBox.Show("Select any process");
                    else
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
                }
            ));
        }

       

    }
}
