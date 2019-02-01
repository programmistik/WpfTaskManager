using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfTaskManager
{
    public class ProcessItem
    {
        public string Name { get; set; }
        public int Pid { get; set; }
        public string UserName { get; set; }
        public decimal Cpu { get; set; }
        public double Ram { get; set; }
        public BitmapFrame Img { get; set; }
    }
}
