using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ComponentModel;

namespace DoublePendulumSimulator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window {

        private bool start = false;
        private bool startSimulation = false;
        private double g = 1;

        private double a1_v = 0;
        private double a2_v = 0;
        
        private double a1;
        private double a2;

        private double start_a1;
        private double start_a2;

        private Data data;


        private RotateTransform pendulum1RotateTransform = new RotateTransform();
        private RotateTransform pendulum2RotateTransform = new RotateTransform();

        public MainWindow() {
            InitializeComponent();
            Angle1.Value = Math.PI/2;
            Angle2.Value = Math.PI/8;
            a1 = (float)Angle1.Value;
            a2 = (float)Angle2.Value;
            start = true;

            SetPendulums();

            pendulum1.RenderTransform = pendulum1RotateTransform;
            pendulum2.RenderTransform = pendulum2RotateTransform;
        }

        private void LoadSimulation(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true) {
                string json = File.ReadAllText(openFileDialog.FileName);
                Data data = JsonConvert.DeserializeObject<Data>(json);
                

                // Colors
                R_Color1.Value = data.color1.R; G_Color1.Value = data.color1.G; B_Color1.Value = data.color1.B;
                R_Color2.Value = data.color1.R; G_Color2.Value = data.color2.G; B_Color2.Value = data.color2.B;

                // set lengths
                Length1.Value = Length1.Value;
                Length2.Value = Length2.Value;

                //set thickness
                Thickness1.Value = data.thickness1;
                Thickness2.Value = data.thickness2;

                // set angles
                Angle1.Value = data.angle1;
                Angle2.Value = data.angle2;

                a1 = data.angle1;
                a2 = data.angle2;
            }
        }

        private void SaveSimulation(object sender, RoutedEventArgs e) {

            // check errors

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Json files (*.json)|*.json";
            if (saveFileDialog.ShowDialog() == true) {
                string json = JsonConvert.SerializeObject(data);
                File.WriteAllText($"{saveFileDialog.FileName}", json);

            }
        }

        private void StartSimulation(object sender, RoutedEventArgs e) {
            if (!startSimulation) {
                startSimulation = true;
                data = new Data() { length1 = Length1.Value, length2 = Length2.Value, thickness1 = Thickness1.Value, thickness2 = Thickness2.Value, color1 = Color.FromArgb((byte)255, (byte)(int)R_Color1.Value, (byte)(int)G_Color1.Value, (byte)(int)B_Color1.Value), color2 = Color.FromArgb((byte)255, (byte)(int)R_Color2.Value, (byte)(int)G_Color2.Value, (byte)(int)B_Color2.Value), angle1 = Angle1.Value, angle2 = Angle2.Value };
                StartButton.Content = "Stop";
                CompositionTarget.Rendering += UpdatePendulums;
            } else {
                StartButton.Content = "Start";
                startSimulation = false;
            }
            
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void UpdatePendulums(object sender, EventArgs e) {
            if (!startSimulation)
                return;
            double m1 = 40;
            double m2 = 40;

            double r1 = 50;
            float r2 = 50;

            double num1 = -g * (2 * m1 + m2) * Math.Sin(a1);
            double num2 = -m2 * g * Math.Sin(a1 - 2 * a2);
            double num3 = -2 * Math.Sin(a1 - a2) * m2;
            double num4 = a2_v * a2_v * r2 + a1_v * a1_v * r1 * Math.Cos(a1 - a2);
            Trace.WriteLine("");
            Trace.WriteLine($" {a2_v} * {a2_v} * {r2}  * {a1_v} * {r1} * {Math.Cos(a1 - a2)}");
            double den = r1 * (2 * m1 + m2 - m2 * Math.Cos(2 * a1 - 2 * a2));
            double a1_a = (num1 + num2 + num3 * num4) / den;
            
            Trace.WriteLine($"{num1}, {num2}, {num3}, {num4}, {den}");
            num1 = 2 * Math.Sin(a1 - a2);
            num2 = a1_v * a1_v * r1 * (m1 + m2);
            num3 = g * (m1 + m2) * Math.Cos(a1);
            num4 = a2_v * a2_v * r2 * m2 * Math.Cos(a1 - a2);
            den = r2 * (2 * m1 + m2 - m2 * Math.Cos(2 * a1 - 2 * a2));
            double a2_a = num1 * (num2 + num3 + num4) / den;

            a1_v += a1_a;
            a2_v += a2_a;

            a1 += a1_v;
            a2 += a2_v;

            PCanvas.Width = PCanvas.ActualWidth;
            PCanvas.Height = PCanvas.ActualHeight;

            // friction
            a1_v *= 1;
            a2_v *= 1;

            SetLines();
        }
       

        private void SetPendulums() {
            pendulum1.Visibility = System.Windows.Visibility.Visible;
            pendulum1.Visibility = System.Windows.Visibility.Visible;

            // stroke thickness
            pendulum1.StrokeThickness = (int)Thickness1.Value;
            pendulum2.StrokeThickness = (int)Thickness2.Value;

            // colors
            pendulum1.Stroke = new SolidColorBrush(Color.FromArgb((byte)255, (byte)(int)R_Color1.Value, (byte)(int)G_Color1.Value, (byte)(int)B_Color1.Value));
            pendulum2.Stroke = new SolidColorBrush(Color.FromArgb((byte)255, (byte)(int)R_Color2.Value, (byte)(int)G_Color2.Value, (byte)(int)B_Color2.Value));
            

            // positions            
            SetLines();
        }

        private void SetLines() {
            // first pendulum

            pendulum1.X2 = 0;
            pendulum1.Y1 = 0;

            pendulum1.X2 = Length1.Value;
            pendulum1.Y2 = Length1.Value;

            Trace.WriteLine(a1 * 180 / Math.PI);
            pendulum1RotateTransform.Angle = 45 - a1 * 180 / Math.PI;
            pendulum1RotateTransform.CenterX = 0;
            pendulum1RotateTransform.CenterY = 0;
            
            Point newP1 = pendulum1RotateTransform.Transform(new Point(pendulum1.X2, pendulum1.Y2));

            pendulum2.X1 = newP1.X;
            pendulum2.Y1 = newP1.Y;

            pendulum2.X2 = pendulum2.X1 + Length2.Value;
            pendulum2.Y2 = pendulum2.Y1 + Length2.Value;

            pendulum2RotateTransform.Angle = 45 - a2 * 180 / Math.PI;
            pendulum2RotateTransform.CenterX = pendulum2.X1;
            pendulum2RotateTransform.CenterY = pendulum2.Y1;
        }


        private void SetLinesFromSlider(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (start) {
                if (sender.GetType() == typeof(Slider)) {
                    a1 = Angle1.Value;
                    a2 = Angle2.Value;
                }
                SetLines();
            }
        }

        private void SetThickness1(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (start)
                pendulum1.StrokeThickness = (int)Thickness1.Value;
        }
        private void SetThickness2(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (start)
                pendulum2.StrokeThickness = (int)Thickness2.Value;
        }

        private void SetColor1(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (start)
                pendulum1.Stroke = new SolidColorBrush(Color.FromArgb((byte)255, (byte)(int)R_Color1.Value, (byte)(int)G_Color1.Value, (byte)(int)B_Color1.Value));
        }
        private void SetColor2(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (start)
                pendulum2.Stroke = new SolidColorBrush(Color.FromArgb((byte)255, (byte)(int)R_Color2.Value, (byte)(int)G_Color2.Value, (byte)(int)B_Color2.Value));
        }
    }

    public class Data {

        // lengths
        public double length1;
        public double length2;

        // thicknesses
        public double thickness1;
        public double thickness2;

        // colors
        public Color color1;
        public Color color2;

        // angles
        public double angle1;
        public double angle2;
    }
}
