using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ReadVolumeFile
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private readonly IEnumerable<BitmapSource> _StackedImages;
        private int _CurrentIndex;

        public MainViewModel()
        {
            var stackedFileReader = new StackedImagesReader("..//..//TestFile.Bin");
            _StackedImages = stackedFileReader.ReadAsSliceImages(1024, 126, 126, 125, 2);

            stackedFileReader.ReadUsing3DArray(1024, 126, 126, 125);

            unsafe
            {
                //stackedFileReader.ReadUsingAllocHGlobal(1024, 126, 126, 125);

                //stackedFileReader.ReadUsingNativeAlloc(1024, 126, 126, 125);

                stackedFileReader.ReadVolumeUsingPInvoke(1024, 126, 126, 125);
            }
        }

        public int TotalImagesCount
        {
            get
            {
                return _StackedImages.Count() - 1;
            }
        }


        public ImageSource ImageSource
        {
            get
            {
                return _StackedImages.ElementAt(CurrentIndex);
            }
        }

        public int CurrentIndex
        {
            get
            {
                return _CurrentIndex;
            }
            set
            {
                _CurrentIndex = value;
                OnPropertyChanged("CurrentIndex");
                OnPropertyChanged("ImageSource");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}