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
            progressLabel.Visibility = Visibility.Visible;
        }


        private void loadDocumentBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Image documents (.png)|*.png";
            
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                ocrData.clearData();

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

                processBtn.IsEnabled = true;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ocrData.doOCR(filePath);
            }
            catch (Exception excp)
            {
                Trace.TraceError(excp.ToString());
                MessageBox.Show(excp.ToString(), "Error during OCR", MessageBoxButton.OK);
            }

            fillListViewWithOCRData();
        }

        private void fillListViewWithOCRData()
        {
            var gridView = new GridView();
            listView.View = gridView;

            gridView.Columns.Add(new GridViewColumn { Header = "Name", DisplayMemberBinding = new Binding("filename") });
            gridView.Columns.Add(new GridViewColumn { Header = "Created", DisplayMemberBinding = new Binding("created") });
            gridView.Columns.Add(new GridViewColumn { Header = "From", DisplayMemberBinding = new Binding("from") });
            gridView.Columns.Add(new GridViewColumn { Header = "To", DisplayMemberBinding = new Binding("to") });
            gridView.Columns.Add(new GridViewColumn { Header = "Topic", DisplayMemberBinding = new Binding("topic") });
            gridView.Columns.Add(new GridViewColumn { Header = "Date", DisplayMemberBinding = new Binding("date") });
            gridView.Columns.Add(new GridViewColumn { Header = "Type", DisplayMemberBinding = new Binding("type") });

            listView.Items.Add(new MyOCRData(ocrData));
        }
    }
}
