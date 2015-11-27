using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tesseract;

namespace ocr_demo
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string filePath = "";

        MyOCRData ocrData;
        
        public MainWindow()
        {
            InitializeComponent();
            ocrData = new MyOCRData();
        }


        private void loadDocumentBtn_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "Image documents (.png)|*.png";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                filePath = dlg.FileName;
                sourcePathTxtBox.Text = filePath;

                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(filePath);
                bi.EndInit();

                sourceImage.Stretch = Stretch.Uniform;
                sourceImage.Source = bi;

                ocrData.filename = Regex.Match(filePath, @".*\\([^\\]+$)").Groups[1].Value;
                ocrData.created = File.GetCreationTime(filePath);

            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(filePath))
                    {
                        var i = 1;
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                Trace.TraceError(excp.ToString());
                Console.WriteLine("Unexpected Error: " + excp.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(excp.ToString());
            }

            fillListViewWithOCRData();
        }

        private void fillListViewWithOCRData()
        {
            var gridView = new GridView();
            listView.View = gridView;

            gridView.Columns.Add(new GridViewColumn { Header = "Name", DisplayMemberBinding = new Binding("filename") });
            gridView.Columns.Add(new GridViewColumn { Header = "Created", DisplayMemberBinding = new Binding("created") });


            listView.Items.Add(new MyOCRData(ocrData));
        }
    }
}
