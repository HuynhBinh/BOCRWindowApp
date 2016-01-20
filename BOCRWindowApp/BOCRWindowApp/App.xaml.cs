using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Media.Ocr;
using System.Threading.Tasks;
using System.Text;
using Windows.Storage.Streams;
using Model;

namespace BOCRWindowApp
{

    sealed partial class App : Application
    {

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }


        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            List<MyLine> listLines = new List<MyLine>();

            //int minSize = 40;
            //int maxSize = 2600;
            int wordTopRange = 25;

            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = null;

            if (!string.IsNullOrEmpty(e.Arguments))
            {
                try
                {
                    OcrEngine ocrEngine = new OcrEngine(OcrLanguage.English);

                    file = await folder.GetFileAsync(e.Arguments);
                    ImageProperties imgProp = await file.Properties.GetImagePropertiesAsync();

                    //if (imgProp.Height < minSize || imgProp.Height > maxSize || imgProp.Width < minSize || imgProp.Width > maxSize)
                    //{
                    //    await WriteToFile(folder, file.Name + ".txt", "Image size must be > 40 and < 2600 pixel");
                    //}
                    //else
                    //{
                    WriteableBitmap bitmap = null;

                    using (IRandomAccessStream imgStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        bitmap = new WriteableBitmap((int)imgProp.Width, (int)imgProp.Height);
                        bitmap.SetSource(imgStream);
                    }

                    // This main API call to extract text from image.
                    OcrResult ocrResult = await ocrEngine.RecognizeAsync((uint)bitmap.PixelHeight, (uint)bitmap.PixelWidth, bitmap.PixelBuffer.ToArray());

                    // If there is text. 
                    if (ocrResult.Lines != null)
                    {
                        StringBuilder builder = new StringBuilder();

                        // loop over recognized text.
                        foreach (OcrLine line in ocrResult.Lines)
                        {
                            // Iterate over words in line.
                            foreach (OcrWord word in line.Words)
                            {
                                // sort the word line by line
                                bool isBelongToLine = false;
                                foreach (MyLine myLine in listLines)
                                {
                                    // if line exist, add word to line
                                    if (Between(myLine.lineNumber, word.Top - wordTopRange, word.Top + wordTopRange, true))
                                    {
                                        myLine.listWords.Add(word);
                                        isBelongToLine = true;
                                        break;
                                    }
                                }

                                // if line does not exist, create new line, add word to line
                                if (isBelongToLine == false)
                                {
                                    MyLine myLine = new MyLine();
                                    myLine.lineNumber = word.Top;
                                    myLine.listWords.Add(word);
                                    listLines.Add(myLine);
                                }
                                // sort the word line by line
                            }
                        }

                        // sort the lines base on top position
                        listLines.Sort();

                        // return data line by line
                        foreach (MyLine myLine in listLines)
                        {
                            builder.Append(myLine.ToString());
                        }


                        await WriteToFile(folder, file.Name + ".txt", builder.ToString());
                    }
                    else // if no text
                    {
                        await WriteToFile(folder, file.Name + ".txt", "No Text");
                    }

                    //}

                }
                catch (Exception ex)
                {
                    await WriteToFile(folder, file.Name + ".txt", "Exception");
                }

                App.Current.Exit();
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private bool Between(int num, int lower, int upper, bool inclusive = false)
        {
            return inclusive
                ? lower <= num && num <= upper
                : lower < num && num < upper;
        }

        public async Task WriteToFile(StorageFolder folder, string fileName, string extractedText)
        {
            // Get the text data from the textbox. 
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(extractedText);

            // Create a new file named DataFile.txt.
            StorageFile file = await folder.CreateFileAsync(fileName);

            // Write the data from the textbox.
            using (Stream s = await file.OpenStreamForWriteAsync())
            {
                s.Write(fileBytes, 0, fileBytes.Length);
            }
        }
    }
}
