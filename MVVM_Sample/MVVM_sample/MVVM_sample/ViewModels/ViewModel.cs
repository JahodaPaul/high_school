using System;
using MVVM_test1.Model;
using MVVM_test1.Base;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace MVVM_test1.ViewModels
{
    class ViewModel : ViewModelBase
    {
        /*Collection that implements interfaces INotifyCollectionChanged, INotifyPropertyChanged 
        As such it is very useful when you want to know when the collection has changed.
        An event is triggered that will tell the user what entries have been added/removed or moved.*/
        private ObservableCollection<DataModel> _data_model;
        private string _text;
        private string _converted_text;
        /*Thanks to UpdateSourceTrigger="RaisePropertyChanged" and Binding to "text" in MainWindowView.xaml
        whenever text in TextBox changes, RaisePropertyChanged calls UI control in View through Binding
        and updates changes*/
        public string text { get { return _text; } set{ _text = value; RaisePropertyChanged("text");} }
        /* (for better understanding I recommend deleting RaisePropertyChanged("converted_text");
        and after running the code and trying the functionality, adding it back to try it again.)*/
        public string converted_text { get { return _converted_text; }
        set { _converted_text = value; _data_model[0].saveText(value);
        _data_model[0].UpdateData(); RaisePropertyChanged("converted_text");} }

        public ViewModel()
        {
            _data_model = new ObservableCollection<DataModel>();
            _data_model.Add(new DataModel());
        }
        /* When the Button is clicked, it calls the ICommand.Execute method of the object bound to its 
        Command property. Binding is done in xaml in MainWindowView.xaml */
        public ICommand ConvertCommand
        {
            get { return new RelayCommand(ConvertText);}
        }
        /* Thanks to interface IEnumerable we can iterate over elements in generic collections. Because we are using
        ObservableCollection and not simple List, when data in collection changes it calls all its listeners(returnList
        in this case) and since we have Binding between ListBox and returnList, the ListBox receives updated collection 
        of data.*/
        public IEnumerable<string> returnList
        {
            get { return _data_model[0].collection_of_converted_texts; }
        }
        public void ConvertText()
        {
            converted_text = _text.ToUpper();
        }
    }
}
//Pavel Jahoda