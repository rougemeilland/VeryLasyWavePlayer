using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WavePlayer.GUI
{
    public class WaveViewGridLineViewModel
        : ViewModel
    {
        private double timeSeconds;
        private Brush color;
        private double thickness;

        public double TimeSeconds
        {
            get => timeSeconds;
            
            set
            {
                timeSeconds = value;
            }
        }

        public Brush Color
        {
            get => color;
            
            set
            {
                color = value;
            }
        }

        public double Thickness
        {
            get => thickness;
            
            set
            {
                thickness = value;
            }
        }
    }
}
