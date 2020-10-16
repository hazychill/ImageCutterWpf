﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using Drawing = System.Drawing;
using IO = System.IO;
using System.Windows.Media.Imaging;

namespace ImageCutterWpf {
    class ImageCutterData : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private string imageSource;
        private double fieldWidth;
        private double fieldHeight;
        private Color fieldBackgroundColor;
        private Brush fieldBackgroundBrush;
        private Color fieldBorderColor;
        private Brush fieldBorderBrush;
        private double previewImageLeft;
        private double previewImageTop;
        private double previewImageWidth;
        private double previewImageHeight;
        private double imageCutBoxLeft;
        private double imageCutBoxTop;
        private double imageCutBoxWidth;
        private double imageCutBoxHeight;
        private IO.MemoryStream streamSource;
        //private byte[] originalImage;
        private BitmapImage originalImage;
        private int outputWidth;
        private int outputHeight;
        private double imageCutBoxVCenterLineX;
        private double imageCutBoxVCenterLineY1;
        private double imageCutBoxVCenterLineY2;
        private double imageCutBoxHCenterLineX1;
        private double imageCutBoxHCenterLineY;
        private double imageCutBoxHCenterLineX2;

        public ImageCutterData() {
            imageSource = null;
            fieldWidth = 0;
            fieldHeight = 0;
            fieldBackgroundColor = Colors.White;
            fieldBackgroundBrush = new SolidColorBrush(fieldBackgroundColor);
            fieldBorderColor = Colors.DarkGray;
            fieldBorderBrush = new SolidColorBrush(fieldBorderColor);
            //originalImage = dummyImageBytes.Clone() as byte[];
            originalImage = null;
        }

        #region Properties for binding

        //public byte[] OriginalImage {
        public BitmapImage OriginalImage {
            get { return originalImage; }
            set {
                originalImage = value;
                OnPropertyChanged("OriginalImage");
            }
        }

        public double FieldWidth {
            get { return fieldWidth; }
            set {
                fieldWidth = value;
                OnPropertyChanged("FieldWidth");
            }
        }

        public double FieldHeight {
            get { return fieldHeight; }
            set {
                fieldHeight = value;
                OnPropertyChanged("FieldHeight");
            }
        }

        public Brush FieldBackgroundBrush {
            get { return fieldBackgroundBrush; }
            set {
                fieldBackgroundBrush = value;
                OnPropertyChanged("FieldBackgroundBrush");
            }
        }

        public Brush FieldBorderBrush {
            get { return fieldBorderBrush; }
            set {
                fieldBorderBrush = value;
                OnPropertyChanged("FieldBorderBrush");
            }
        }

        public double PreviewImageLeft {
            get { return previewImageLeft; }
            set {
                previewImageLeft = value;
                OnPropertyChanged("PreviewImageLeft");
            }
        }

        public double PreviewImageTop {
            get { return previewImageTop; }
            set {
                previewImageTop = value;
                OnPropertyChanged("PreviewImageTop");
            }
        }

        public double PreviewImageWidth {
            get { return previewImageWidth; }
            set {
                previewImageWidth = value;
                OnPropertyChanged("PreviewImageWidth");
            }
        }

        public double PreviewImageHeight {
            get { return previewImageHeight; }
            set {
                previewImageHeight = value;
                OnPropertyChanged("PreviewImageHeight");
            }
        }

        public IO.MemoryStream PreviewImageStreamSource {
            get { return streamSource; }
            set {
                streamSource = value;
                OnPropertyChanged("PreviewImageStreamSource");
            }
        }

        public double ImageCutBoxLeft {
            get { return imageCutBoxLeft; }
            set {
                imageCutBoxLeft = value;
                OnPropertyChanged("ImageCutBoxLeft");
                UpdateImageCutBoxCenterLines();
            }
        }

        public double ImageCutBoxTop {
            get { return imageCutBoxTop; }
            set {
                imageCutBoxTop = value;
                OnPropertyChanged("ImageCutBoxTop");
                UpdateImageCutBoxCenterLines();
            }
        }

        public double ImageCutBoxWidth {
            get { return imageCutBoxWidth; }
            set {
                imageCutBoxWidth = value;
                OnPropertyChanged("ImageCutBoxWidth");
                UpdateImageCutBoxCenterLines();
            }
        }

        public double ImageCutBoxHeight {
            get { return imageCutBoxHeight; }
            set {
                imageCutBoxHeight = value;
                OnPropertyChanged("ImageCutBoxHeight");
                UpdateImageCutBoxCenterLines();
            }
        }

        public double ImageCutBoxVCenterLineX {
            get { return imageCutBoxVCenterLineX; }
            set {
                imageCutBoxVCenterLineX = value;
                OnPropertyChanged("ImageCutBoxVCenterLineX");
            }
        }

        public double ImageCutBoxVCenterLineY1 {
            get { return imageCutBoxVCenterLineY1; }
            set {
                imageCutBoxVCenterLineY1 = value;
                OnPropertyChanged("ImageCutBoxVCenterLineY1");
            }
        }

        public double ImageCutBoxVCenterLineY2 {
            get { return imageCutBoxVCenterLineY2; }
            set {
                imageCutBoxVCenterLineY2 = value;
                OnPropertyChanged("ImageCutBoxVCenterLineY2");
            }
        }

        public double ImageCutBoxHCenterLineX1 {
            get { return imageCutBoxHCenterLineX1; }
            set {
                imageCutBoxHCenterLineX1 = value;
                OnPropertyChanged("ImageCutBoxHCenterLineX1");
            }
        }

        public double ImageCutBoxHCenterLineY {
            get { return imageCutBoxHCenterLineY; }
            set {
                imageCutBoxHCenterLineY = value;
                OnPropertyChanged("ImageCutBoxHCenterLineY");
            }
        }

        public double ImageCutBoxHCenterLineX2 {
            get { return imageCutBoxHCenterLineX2; }
            set {
                imageCutBoxHCenterLineX2 = value;
                OnPropertyChanged("ImageCutBoxHCenterLineX2");
            }
        }

        #endregion

        public string ImageSource {
            get { return imageSource; }
            set {
                imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        public Color FieldBackgroundColor {
            get { return fieldBackgroundColor; }
            set {
                fieldBackgroundColor = value;
                FieldBackgroundBrush = new SolidColorBrush(fieldBackgroundColor);
            }
        }

        public Color FieldBorderColor {
            get { return fieldBorderColor; }
            set {
                fieldBorderColor = value;
                fieldBorderBrush = new SolidColorBrush(fieldBorderColor);
            }
        }

        public byte[] OriginalImageBytes {
            get {
                if (originalImage != null) {
                    var ms = originalImage.StreamSource as IO.MemoryStream;
                    if (ms != null) {
                        return ms.ToArray();
                    }
                    else {
                        return null;
                    }
                }
                else {
                    return null;
                }
            }
            set {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = new IO.MemoryStream(value); ;
                image.EndInit();
                OriginalImage = image;
            }
        }

        public int OutputWidth {
            get { return outputWidth; }
            set { outputWidth = value; }
        }

        public int OutputHeight {
            get { return outputHeight; }
            set { outputHeight = value; }
        }

        private void OnPropertyChanged(string name) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void UpdateImageCutBoxCenterLines() {
            ImageCutBoxVCenterLineX = imageCutBoxLeft + imageCutBoxWidth / 2;
            ImageCutBoxVCenterLineY1 = imageCutBoxTop;
            ImageCutBoxVCenterLineY2 = imageCutBoxTop + imageCutBoxHeight;

            ImageCutBoxHCenterLineX1 = imageCutBoxLeft;
            ImageCutBoxHCenterLineX2 = imageCutBoxLeft + imageCutBoxWidth;
            ImageCutBoxHCenterLineY = imageCutBoxTop + imageCutBoxWidth / 2;
        }

        #region Dummy image

        byte[] dummyImageBytes = {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 
            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x03, 0x00, 0x00, 0x00, 0x28, 0xCB, 0x34, 0xBB, 0x00, 
            0x00, 0x00, 0x01, 0x73, 0x52, 0x47, 0x42, 0x00, 0xAE, 0xCE, 0x1C, 0xE9, 0x00, 0x00, 0x00, 0x04, 0x67, 
            0x41, 0x4D, 0x41, 0x00, 0x00, 0xB1, 0x8F, 0x0B, 0xFC, 0x61, 0x05, 0x00, 0x00, 0x00, 0x20, 0x63, 0x48, 
            0x52, 0x4D, 0x00, 0x00, 0x7A, 0x26, 0x00, 0x00, 0x80, 0x84, 0x00, 0x00, 0xFA, 0x00, 0x00, 0x00, 0x80, 
            0xE8, 0x00, 0x00, 0x75, 0x30, 0x00, 0x00, 0xEA, 0x60, 0x00, 0x00, 0x3A, 0x98, 0x00, 0x00, 0x17, 0x70, 
            0x9C, 0xBA, 0x51, 0x3C, 0x00, 0x00, 0x03, 0x00, 0x50, 0x4C, 0x54, 0x45, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x23, 
            0xB7, 0xE1, 0x00, 0x00, 0x00, 0x1A, 0x74, 0x45, 0x58, 0x74, 0x53, 0x6F, 0x66, 0x74, 0x77, 0x61, 0x72, 
            0x65, 0x00, 0x50, 0x61, 0x69, 0x6E, 0x74, 0x2E, 0x4E, 0x45, 0x54, 0x20, 0x76, 0x33, 0x2E, 0x35, 0x2E, 
            0x31, 0x31, 0x47, 0xF3, 0x42, 0x37, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x18, 0x57, 0x63, 
            0x60, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01, 0xA3, 0xDA, 0x3D, 0x94, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 
            0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };

        #endregion
    }
}