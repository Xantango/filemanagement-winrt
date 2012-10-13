using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace FileStreamAccess
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        StorageFile sampleFile;
        const string fileName = "Sample.data";
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void CreateFileClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            this.sampleFile = await storageFolder.CreateFileAsync(
                MainPage.fileName, CreationCollisionOption.ReplaceExisting);
            StatusTextBlock.Text = "The file '" + sampleFile.Name + "' was created.";
        }

        private async void OpenReadClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (this.sampleFile == null)
                {
                    await OpenFile();
                }
                if (this.sampleFile != null)
                {
                    await ReadFromFile();
                }
                else
                {
                    StatusTextBlock.Text = @"Invalid file handle. Please create the file first.";
                }
            }
            catch (FileNotFoundException)
            {
                StatusTextBlock.Text = "File does not exist";
            }
        }

        private async System.Threading.Tasks.Task OpenFile()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            this.sampleFile = await storageFolder.GetFileAsync(MainPage.fileName);
            StatusTextBlock.Text = "The file '" + sampleFile.Name + "' was loaded.";
        }

        private async System.Threading.Tasks.Task ReadFromFile()
        {
            using (IRandomAccessStream sessionRandomAccess =
                await this.sampleFile.OpenAsync(FileAccessMode.Read))
            {
                if (sessionRandomAccess.Size > 0)
                {
                    byte[] array3 = new byte[sessionRandomAccess.Size];
                    IBuffer output = await
                                sessionRandomAccess.ReadAsync(
                                    array3.AsBuffer(0, (int)sessionRandomAccess.Size),
                                    (uint)sessionRandomAccess.Size,
                                    InputStreamOptions.Partial);
                    string reRead = Encoding.UTF8.GetString(output.ToArray(), 0, (int)output.Length);
                    StatusTextBlock.Text =
                        "The following text was read from '" + this.sampleFile.Name +
                        "' using a stream:" + Environment.NewLine + Environment.NewLine + reRead;
                }
                else
                {
                    StatusTextBlock.Text = "File is empty";
                }
            }
        }

        private async void DeleteClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                this.ResetScenarioOutput();
                StorageFile file = this.sampleFile;
                if (file == null)
                {
                    await OpenFile();
                }
                if (file != null)
                {
                    string filename = file.Name;
                    await file.DeleteAsync();
                    this.sampleFile = null;
                    StatusTextBlock.Text = "The file '" + filename + "' was deleted";
                }
                else
                {
                    StatusTextBlock.Text = "The file does not exist";
                }
            }
            catch (FileNotFoundException)
            {
                this.NotifyUserFileNotExist();
            }
        }

        private void NotifyUserFileNotExist()
        {
            StatusTextBlock.Text = "File does not exist";
        }

        private void ResetScenarioOutput()
        {
            this.StatusTextBlock.Text = "";
            this.InputTextBox.Text = "";

        }

        private async void AppendSaveClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StorageFile file = this.sampleFile;
            if (file != null)
            {
                string userContent = InputTextBox.Text;
                if (!String.IsNullOrEmpty(userContent))
                {
                    using (IRandomAccessStream sessionRandomAccess =
                        await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        Stream stream = sessionRandomAccess.AsStreamForWrite();
                        if (stream.Length > 0)
                        {
                            stream.Seek(stream.Length, SeekOrigin.Begin);
                        }
                        byte[] array = Encoding.UTF8.GetBytes(userContent);
                        stream.SetLength(stream.Length + array.Length);

                        await stream.WriteAsync(array, 0, array.Length);
                        await stream.FlushAsync();

                        await sessionRandomAccess.FlushAsync();
                    }
                    ResetScenarioOutput();
                    await ReadFromFile();
                }
                else
                {
                    StatusTextBlock.Text = @"The text box is empty, please write something
                        and then click 'Write' again.";
                }
            }
            else
            {
                StatusTextBlock.Text = @"File not open!";
            }
        }
    }
}
