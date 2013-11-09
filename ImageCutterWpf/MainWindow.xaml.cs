using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using IO = System.IO;
using WinForms = System.Windows.Forms;
using Drawing = System.Drawing;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace ImageCutterWpf {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        const int MAX_IMAGE_FILE_LENGTH_BYTES = 31457280;
        const int MAX_OUTPUT_WIDTH = 100000;
        const int MAX_OUTPUT_HEIGHT = 100000;
        const string SETTINGS_FILE_NAME = "settings.txt";
        const string DEFAULT_SETTINGS_JSON = "{  \"outputSize\": {    \"width\": 100,    \"height\": 100  },  \"backgroundColor\": {    \"a\": 255,    \"r\": 255,    \"g\": 255,    \"b\": 255  },  \"window\": {    \"width\": 500.0,    \"height\": 500.0,    \"left\": 0.0,    \"top\": 0.0,    \"state\": \"Normal\"  }}";

        ImageCutterData data;
        bool dragging;
        double dragStartMouseLeft;
        double dragStartMouseTop;
        double dragStartImageCutBoxLeft;
        double dragStartImageCutBoxTop;

        public MainWindow() {
            InitializeComponent();
            data = new ImageCutterData() {
                
            };
            this.DataContext = data;
            dragging = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            outputWidthTextBox.Background = Brushes.White;
            outputHeightTextBox.Background = Brushes.White;

            ApplySettings();

            OnAplyOutputSizeButtonClick();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            SetControlSize();
        }

        private void SetControlSize() {
            if (mainCanvas == null) {
                return;
            }
            var fieldWidth = mainCanvas.RenderSize.Width;
            var fieldHeight = mainCanvas.RenderSize.Height;
            data.FieldWidth = fieldWidth;
            data.FieldHeight = fieldHeight;

            if (data.OriginalImage == null) {
                return;
            }

            var oiWidth = (double)data.OriginalImage.PixelWidth;
            var oiHeight = (double)data.OriginalImage.PixelHeight;

            double piWidth;
            double piHeight;
            if (fieldWidth * oiHeight > fieldHeight * oiWidth) {
                piHeight = fieldHeight * previewImageScaleSlider.Value / previewImageScaleSlider.Maximum;
                piWidth = oiWidth * piHeight / oiHeight;
            }
            else {
                piWidth = fieldWidth * previewImageScaleSlider.Value / previewImageScaleSlider.Maximum;
                piHeight = oiHeight * piWidth / oiWidth;
            }
            double piLeft = (fieldWidth - piWidth) / 2;
            double piTop = (fieldHeight - piHeight) / 2;
            data.PreviewImageWidth = piWidth;
            data.PreviewImageHeight = piHeight;
            data.PreviewImageLeft = piLeft;
            data.PreviewImageTop = piTop;

            double opWidth = data.OutputWidth;
            double opHeight = data.OutputHeight;
            double icbWidth;
            double icbHeight;
            if (fieldWidth * opHeight > fieldHeight * opWidth) {
                icbHeight = fieldHeight * imageCutBoxScaleSlider.Value / imageCutBoxScaleSlider.Maximum;
                icbWidth = opWidth * icbHeight / opHeight;
            }
            else {
                icbWidth = fieldWidth * imageCutBoxScaleSlider.Value / imageCutBoxScaleSlider.Maximum;
                icbHeight = opHeight * icbWidth / opWidth;
            }
            var icbLeft = data.ImageCutBoxLeft;
            var icbTop = data.ImageCutBoxTop;
            if (icbLeft + icbWidth > fieldWidth) {
                icbLeft = fieldWidth - icbWidth;
            }
            if (icbLeft < 0) {
                icbLeft = 0;
            }
            if (icbTop + icbHeight > fieldHeight) {
                icbTop = fieldHeight - icbHeight;
            }
            if (icbTop < 0) {
                icbTop = 0;
            }
            if (icbWidth < 0) {
                icbWidth = 0;
            }
            if (icbWidth > fieldWidth) {
                icbWidth = fieldWidth;
            }
            if (icbHeight < 0) {
                icbHeight = 0;
            }
            if (icbHeight > fieldHeight) {
                icbHeight = fieldHeight;
            }
            data.ImageCutBoxLeft = icbLeft;
            data.ImageCutBoxTop = icbTop;
            data.ImageCutBoxWidth = icbWidth;
            data.ImageCutBoxHeight = icbHeight;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {

        }

        private void openImageMenuItem_Click(object sender, RoutedEventArgs e) {
            using (var openDialog = new WinForms.OpenFileDialog()) {
                openDialog.CheckFileExists = true;
                openDialog.CheckPathExists = true;
                openDialog.Filter = "Image files|*.jpg;*.png;*.bmp;*.gif|All files|*.*";
                openDialog.Multiselect = false;
                var result = openDialog.ShowDialog();
                if (result == WinForms.DialogResult.OK) {
                    var fileName = openDialog.FileName;
                    var fileDataBytes = ReadAllBytes(fileName, MAX_IMAGE_FILE_LENGTH_BYTES);
                    data.OriginalImageBytes = fileDataBytes;
                }
            }

            SetControlSize();
        }

        private byte[] ReadAllBytes(string fileName, int MAX_IMAGE_FILE_LENGTH_BYTES) {
            using (var input = IO.File.Open(fileName, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)) {
                var fileLength = ((int)new IO.FileInfo(fileName).Length);
                if (fileLength > MAX_IMAGE_FILE_LENGTH_BYTES) {
                    return null;
                }

                var byteData = new byte[fileLength];
                var totalRead = 0;
                int count;
                int readBufferLength = 4096;
                do {
                    var nextRead = (readBufferLength < fileLength - totalRead) ? (readBufferLength) : (fileLength - totalRead);
                    count = input.Read(byteData, totalRead, nextRead);
                    totalRead += count;
                } while (totalRead < fileLength);

                return byteData;
            }
        }

        private void saveImageMenuItem_Click(object sender, RoutedEventArgs e) {
            if (data.OriginalImage == null) {
                return;
            }
            var ms = data.OriginalImage.StreamSource as IO.MemoryStream;
            if (ms == null) {
                return;
            }
            using (var outputBitmap = new Drawing.Bitmap((int)data.OutputWidth, (int)data.OutputHeight)) {
                using (var g = Drawing.Graphics.FromImage(outputBitmap)) {
                    var color = fieldColorPicker.SelectedColor;
                    var drawingColor = Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
                    g.Clear(drawingColor);

                    var icbLeft = data.ImageCutBoxLeft;
                    var icbTop = data.ImageCutBoxTop;
                    var icbWidth = data.ImageCutBoxWidth;
                    var icbHeight = data.ImageCutBoxHeight;
                    var piLeft = data.PreviewImageLeft;
                    var piTop = data.PreviewImageTop;
                    var piWidth = data.PreviewImageWidth;
                    var piHeight = data.PreviewImageHeight;
                    var opWidth = data.OutputWidth;
                    var opHeight = data.OutputHeight;
                    var oiWidth = data.OriginalImage.PixelWidth;
                    var oiHeight = data.OriginalImage.PixelHeight;

                    var imageCutBoxRect = new Drawing.RectangleF(
                        0f, 0f,
                        (float)icbWidth, (float)icbHeight);
                    var previewImageRect = new Drawing.RectangleF(
                        (float)piLeft - (float)icbLeft, (float)piTop - (float)icbTop,
                        (float)piWidth, (float)piHeight);
                    var intercectedRect = Drawing.RectangleF.Intersect(imageCutBoxRect, previewImageRect);

                    var sizedLeft = opWidth * intercectedRect.Left / icbWidth;
                    var sizedTop = opWidth * intercectedRect.Top / icbWidth;

                    var imageRectangleRendered = Drawing.RectangleF.Intersect(
                        new Drawing.RectangleF(0f, 0f, (float)piWidth, (float)piHeight),
                        new Drawing.RectangleF((float)(icbLeft - piLeft), (float)(icbTop - piTop), (float)icbWidth, (float)icbHeight));
                    var imageRectangleOriginalLeft = oiWidth * imageRectangleRendered.Left / piWidth;
                    var imageRectangleOriginalTop = oiWidth * imageRectangleRendered.Top / piWidth;
                    var imageRectangleOriginalWidth = oiWidth * imageRectangleRendered.Width / piWidth;
                    var imageRectangleOriginalHeight = oiWidth * imageRectangleRendered.Height / piWidth;
                    var imageRectangleOriginal = new Drawing.RectangleF(
                        (float)imageRectangleOriginalLeft, (float)imageRectangleOriginalTop,
                        (float)imageRectangleOriginalWidth, (float)imageRectangleOriginalHeight);

                    var drawRectangleRendered = Drawing.RectangleF.Intersect(
                        new Drawing.RectangleF(0f, 0f, (float)icbWidth, (float)icbHeight),
                        new Drawing.RectangleF((float)(piLeft - icbLeft), (float)(piTop - icbTop), (float)piWidth, (float)piHeight));
                    var drawRectangleOriginalLeft = opWidth * drawRectangleRendered.Left / icbWidth;
                    var drawRectangleOriginalTop = opWidth * drawRectangleRendered.Top / icbWidth;
                    var drawRectangleOriginalWidth = opWidth * drawRectangleRendered.Width / icbWidth;
                    var drawRectangleOriginalHeight = opWidth * drawRectangleRendered.Height / icbWidth;
                    var drawRectangleOriginal = new Drawing.RectangleF(
                        (float)drawRectangleOriginalLeft, (float)drawRectangleOriginalTop,
                        (float)drawRectangleOriginalWidth, (float)drawRectangleOriginalHeight);

                    using (var originalImage = Drawing.Image.FromStream(ms)) {
                        g.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(originalImage, drawRectangleOriginal, imageRectangleOriginal, Drawing.GraphicsUnit.Pixel);
                    }
                }

                using (var saveDialog = new WinForms.SaveFileDialog()) {
                    saveDialog.AddExtension = true;
                    saveDialog.CheckPathExists = true;
                    saveDialog.DefaultExt = "*.png";
                    saveDialog.OverwritePrompt = true;
                    var result = saveDialog.ShowDialog();
                    if (result == WinForms.DialogResult.OK) {
                        var fileName = saveDialog.FileName;
                        Drawing.Imaging.ImageFormat format;
                        var ext = IO.Path.GetExtension(fileName);
                        if (String.Equals(".png", ext, StringComparison.OrdinalIgnoreCase)) {
                            format = Drawing.Imaging.ImageFormat.Png;
                        }
                        else if (String.Equals(".jpg", ext, StringComparison.OrdinalIgnoreCase)) {
                            format = Drawing.Imaging.ImageFormat.Jpeg;
                        }
                        else if (String.Equals(".bmp", ext, StringComparison.OrdinalIgnoreCase)) {
                            format = Drawing.Imaging.ImageFormat.Bmp;
                        }
                        else if (String.Equals(".gif", ext, StringComparison.OrdinalIgnoreCase)) {
                            format = Drawing.Imaging.ImageFormat.Gif;
                        }
                        else {
                            format = Drawing.Imaging.ImageFormat.Png;
                        }
                        outputBitmap.Save(fileName, format);
                    }
                }

            }
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            SetControlSize();
        }

        private void outputSizeTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var textBox = sender as TextBox;
            if (textBox == null) {
                return;
            }
            textBox.Background = Brushes.Yellow;
        }

        private void aplyOutputSizeButton_Click(object sender, RoutedEventArgs e) {
            OnAplyOutputSizeButtonClick();
            SaveSettings();
        }

        private void OnAplyOutputSizeButtonClick() {
            int outputWidth;
            int outputHeight;
            if (!int.TryParse(outputWidthTextBox.Text, out outputWidth)) {
                outputWidthTextBox.Background = Brushes.Red;
                return;
            }
            if (!int.TryParse(outputHeightTextBox.Text, out outputHeight)) {
                outputHeightTextBox.Background = Brushes.Red;
                return;
            }

            if (outputWidth > MAX_OUTPUT_WIDTH) {
                outputWidthTextBox.Background = Brushes.Red;
                return;
            }
            if (outputHeight > MAX_OUTPUT_HEIGHT) {
                outputHeightTextBox.Background = Brushes.Red;
                return;
            }

            data.OutputWidth = outputWidth;
            data.OutputHeight = outputHeight;

            outputWidthTextBox.Background = Brushes.White;
            outputHeightTextBox.Background = Brushes.White;

            SetControlSize();
        }

        private void mainCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
            dragging = true;
            var point = Mouse.GetPosition(sender as IInputElement);
            dragStartMouseLeft = point.X;
            dragStartMouseTop = point.Y;
            dragStartImageCutBoxLeft = data.ImageCutBoxLeft;
            dragStartImageCutBoxTop = data.ImageCutBoxTop;
        }

        private void mainCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
            dragging = false;
        }

        private void mainCanvas_MouseMove(object sender, MouseEventArgs e) {
            if (dragging) {
                var point = Mouse.GetPosition(sender as IInputElement);
                var currentLeft = point.X;
                var currentTop = point.Y;
                var diffLeft = currentLeft - dragStartMouseLeft;
                var diffTop = currentTop - dragStartMouseTop;
                var icbLeft = dragStartImageCutBoxLeft + diffLeft;
                var icbTop = dragStartImageCutBoxTop + diffTop;
                var icbWidth = data.ImageCutBoxWidth;
                var icbHeight = data.ImageCutBoxHeight;
                if (icbLeft < 0) {
                    icbLeft = 0;
                }
                if (icbTop < 0) {
                    icbTop = 0;
                }
                if (icbLeft + icbWidth > data.FieldWidth) {
                    icbLeft = data.FieldWidth - icbWidth;
                }
                if (icbTop + icbHeight > data.FieldHeight) {
                    icbTop = data.FieldHeight - icbHeight;
                }
                data.ImageCutBoxLeft = icbLeft;
                data.ImageCutBoxTop = icbTop;
            }
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e) {
            if (data == null) {
                return;
            }
            var newColor = e.NewValue;
            data.FieldBackgroundColor = newColor;
            SaveSettings();
        }

        private void SaveSettings() {
            var settings = new JObject(
                new JProperty("outputSize",
                    new JObject(
                        new JProperty("width", data.OutputWidth),
                        new JProperty("height", data.OutputHeight))),
                new JProperty("backgroundColor",
                    new JObject(
                        new JProperty("a", fieldColorPicker.SelectedColor.A),
                        new JProperty("r", fieldColorPicker.SelectedColor.R),
                        new JProperty("g", fieldColorPicker.SelectedColor.G),
                        new JProperty("b", fieldColorPicker.SelectedColor.B))),
                new JProperty("window",
                    new JObject(
                        new JProperty("width", this.Width),
                        new JProperty("height", this.Height),
                        new JProperty("left", this.Left),
                        new JProperty("top", this.Top),
                        new JProperty("state", (this.WindowState == System.Windows.WindowState.Minimized) ? (WindowState.Normal.ToString()) : (this.WindowState.ToString())))));
            var settingsFilePath = GetSettingsFilePath();
            var settingsFileDir = IO.Path.GetDirectoryName(settingsFilePath);
            if (IO.Directory.Exists(settingsFileDir)) {
                IO.File.WriteAllText(settingsFilePath, settings.ToString(), new UTF8Encoding());
            }
        }

        private string GetSettingsFilePath() {
            var exePath = Assembly.GetEntryAssembly().Location;
            var exeDir = IO.Path.GetDirectoryName(exePath);
            var settingsFilePath = IO.Path.Combine(exeDir, SETTINGS_FILE_NAME);
            return settingsFilePath;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            SaveSettings();
        }

        private void ApplySettings() {
            string settingsJson;
            var settingsFilePath = GetSettingsFilePath();
            if (!IO.File.Exists(settingsFilePath)) {
                settingsJson = DEFAULT_SETTINGS_JSON;
            }
            else {
                try {
                    settingsJson = IO.File.ReadAllText(settingsFilePath, new UTF8Encoding());
                }
                catch (Exception e) {
                    settingsJson = DEFAULT_SETTINGS_JSON;
                    Debug.WriteLine(e);
                }
            }
            var jobj = JObject.Parse(settingsJson);

            outputWidthTextBox.Text = jobj["outputSize"]["width"].Value<int>().ToString();
            outputHeightTextBox.Text = jobj["outputSize"]["height"].Value<int>().ToString();

            var a = jobj["backgroundColor"]["a"].Value<byte>();
            var r = jobj["backgroundColor"]["r"].Value<byte>();
            var g = jobj["backgroundColor"]["g"].Value<byte>();
            var b = jobj["backgroundColor"]["b"].Value<byte>();
            fieldColorPicker.SelectedColor = Color.FromArgb(a, r, g, b);

            this.Left = jobj["window"]["left"].Value<double>();
            this.Top = jobj["window"]["top"].Value<double>();
            this.Width = jobj["window"]["width"].Value<double>();
            this.Height = jobj["window"]["width"].Value<double>();
            var stateValue = jobj["window"]["state"].Value<string>();
            WindowState state;
            if (Enum.TryParse<WindowState>(stateValue, out state)) {
                this.WindowState = state;
            }
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

    }
}
