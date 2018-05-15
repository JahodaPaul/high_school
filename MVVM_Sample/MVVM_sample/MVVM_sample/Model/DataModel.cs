using System;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;

namespace MVVM_test1.Model
{
    class DataModel
    {
        /*Collection that implements interfaces INotifyCollectionChanged, INotifyPropertyChanged 
        As such it is very useful when you want to know when the collection has changed.
        An event is triggered that will tell the user what entries have been added/removed or moved.*/
        public ObservableCollection<string> collection_of_converted_texts;
        private static readonly string file_name = "data.txt";
        // FileStream for accessing files
        FileStream stream = new FileStream(file_name, FileMode.OpenOrCreate);

        public DataModel()
        {
            GetDataFromTextFile();
        }

        public void saveText(string convertedText)
        {
            collection_of_converted_texts.Add(convertedText);
        }
        public void GetDataFromTextFile()
        {
            // reading data from file
            StreamReader reader = new StreamReader(stream, Encoding.Default);
            collection_of_converted_texts = new ObservableCollection<string>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                collection_of_converted_texts.Add(line);
            }
            reader.Close();
            stream.Close();
        }
        public void UpdateData()
        {
            //overwriting file with new collection of strings
            stream = new FileStream(file_name, FileMode.Create);
            StreamWriter writer = new StreamWriter(stream, Encoding.Default);
            foreach (string item in collection_of_converted_texts)
            {
                writer.WriteLine(item);
            }
            writer.Close();
            stream.Close();
        }
    }
}
//Pavel Jahoda