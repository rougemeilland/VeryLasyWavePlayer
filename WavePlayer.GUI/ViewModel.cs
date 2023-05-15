﻿using System;
using System.ComponentModel;

namespace WavePlayer.GUI
{
    public abstract class ViewModel
        : INotifyPropertyChanged

    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            if (!(PropertyChanged is null))
            {
                try
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
