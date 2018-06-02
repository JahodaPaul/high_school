using System;
using System.ComponentModel;

namespace MVVM_test1.Base
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /* Method RaisePropertyChanged is a helper to raise the event if anyone is listening out for it and
         plays important role in updating binded elements in the view.*/
        protected void RaisePropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
