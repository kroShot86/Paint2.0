using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System;
using System.Windows.Threading;
using System.Windows.Media.Effects;

using System.IO;
using System.Windows.Controls.Primitives;

namespace Paint_2._0
{
    public partial class MainWindow : Window
    {
        private string instrument = "Карандаш";
        public MainWindow()
        {
            InitializeComponent();
            this.SizeChanged += (s, e) => DrawSetkaChange();
        }

        Button activeButton;
        DispatcherTimer timer = new DispatcherTimer();
        bool brighter = true;
        double opacity = 0.3;

        private Point startPoint;

        private bool LKM = false;
        private Brush cwet = Brushes.Black;

        private Polyline karandash;
        private bool karandash_check = true;

        private Polyline lastik;
        private bool lastik_check = false;

        private bool zalivka_check = false;
        private bool text_check = false;

        private Line liniy;
        private bool liniy_check = false;

        private Rectangle pryam;
        private bool pryam_check = false;

        private Ellipse eleps;
        private bool eleps_check = false;

        private Polygon treug;
        private bool treug_check = false;

        private bool setka_check = false;

        private bool vydel_check = false;
        private int count_vydel = 0;
        private Rectangle vydelRect;
        private Point vydelStart;

        private List<Thumb> resizeThumbs = new List<Thumb>();
        private Point dragStart;

        private RenderTargetBitmap copiedImage = null; // фигня для копирования

        private Stack<RenderTargetBitmap> history = new Stack<RenderTargetBitmap>();

        private List<UIElement> selectedObjects = new List<UIElement>();
        private Dictionary<UIElement, Point> originalPositions = new Dictionary<UIElement, Point>();
        private bool isDraggingSelection = false;

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LKM = true;
            Point pos = e.GetPosition(Miro);

            karandash = new Polyline
            {
                Stroke = cwet,
                StrokeThickness = Tolshina.Value,
                Points = new PointCollection()
            };

            lastik = new Polyline
            {
                Stroke = Miro.Background,
                StrokeThickness = Tolshina.Value,
                Points = new PointCollection()
            };

            if (karandash_check)
            {
                Miro.Children.Add(karandash);
            }

            if (lastik_check)
            {
                Miro.Children.Add(lastik);
            }

            if (zalivka_check)
            {
                FloodFill(Miro, e.GetPosition(Miro), ((SolidColorBrush)cwet).Color);
            }

            if (liniy_check)
            {
                liniy = new Line
                {
                    Stroke = cwet,
                    StrokeThickness = Tolshina.Value,
                    X1 = pos.X,
                    Y1 = pos.Y,
                    X2 = pos.X,
                    Y2 = pos.Y
                };
                Miro.Children.Add(liniy);
            
            }

            if (pryam_check)
            {
                startPoint = e.GetPosition(Miro);

                pryam = new Rectangle
                {
                    Stroke = cwet,
                    StrokeThickness = Tolshina.Value,
                    Fill = Brushes.Transparent
                };

                Canvas.SetLeft(pryam, startPoint.X);
                Canvas.SetTop(pryam, startPoint.Y);
                Miro.Children.Add(pryam);
            }

            if (eleps_check)
            {
                startPoint = e.GetPosition(Miro);

                eleps = new Ellipse
                {
                    Stroke = cwet,
                    StrokeThickness = Tolshina.Value,
                    Fill = Brushes.Transparent
                };

                Canvas.SetLeft(eleps, startPoint.X);
                Canvas.SetTop(eleps, startPoint.Y);
                Miro.Children.Add(eleps);
            }

            if (treug_check)
            {
                startPoint = e.GetPosition(Miro);

                treug = new Polygon
                {
                    Stroke = cwet,
                    StrokeThickness = Tolshina.Value,
                    Fill = Brushes.Transparent
                };
                Miro.Children.Add(treug);
            }


            if (text_check)
            {
                text_check = false;

                TextBox textBox = new TextBox
                {
                    FontSize = 24,
                    Foreground = cwet,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    TextAlignment = TextAlignment.Center,
                    AcceptsReturn = false,
                    Width = 150,
                    Height = 30,
                    Text = "Введите текст",
                    CaretBrush = Brushes.Black
                };

                Canvas.SetLeft(textBox, (pos.X - 75));
                Canvas.SetTop(textBox, (pos.Y - 15));

                Miro.Children.Add(textBox);

                void FinalizeText()
                {
                    TextBlock tb = new TextBlock
                    {
                        Text = textBox.Text,
                        Foreground = cwet,
                        FontSize = textBox.FontSize,
                        Background = Brushes.Transparent
                    };
                    Canvas.SetLeft(tb, Canvas.GetLeft(textBox) + Width/20);
                    Canvas.SetTop(tb, Canvas.GetTop(textBox));
                    Miro.Children.Remove(textBox);
                    Miro.Children.Add(tb);
                }

                textBox.KeyDown += (s, ev) =>
                {
                    if (ev.Key == Key.Enter)
                    {
                        FinalizeText();
                    }
                };

                textBox.LostFocus += (s, ev) =>
                {
                    FinalizeText();
                };
            }

            if (vydel_check)
            {
                if (vydelRect != null)
                if (vydelRect.Fill != Miro.Background)
                    Dell_vydel();

                vydelStart = pos;

                vydelRect = new Rectangle
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                };

                Canvas.SetLeft(vydelRect, pos.X);
                Canvas.SetTop(vydelRect, pos.Y);

                Miro.Children.Add(vydelRect);

            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(Miro);
            LKM = false;
            if (pryam_check)
            {
                pryam = null;
            }

            if (eleps_check)
            {
                eleps = null;
            }

            if (treug_check)
            {
                treug = null;
            }

            if (vydel_check)
            {
                count_vydel++;
            }

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(Miro);

            if (!LKM)
                return;

            karandash.Points.Add(e.GetPosition(Miro));
            lastik.Points.Add(e.GetPosition(Miro));

            if (vydel_check && vydelRect != null && LKM)
            {
                double x = Math.Min(pos.X, vydelStart.X);
                double y = Math.Min(pos.Y, vydelStart.Y);
                double w = Math.Abs(pos.X - vydelStart.X);
                double h = Math.Abs(pos.Y - vydelStart.Y);

                Canvas.SetLeft(vydelRect, x);
                Canvas.SetTop(vydelRect, y);
                vydelRect.Width = w;
                vydelRect.Height = h;
            }

            if (liniy_check && liniy != null)
            {
                liniy.X2 = pos.X;
                liniy.Y2 = pos.Y;
            }

            if (pryam_check && pryam != null && LKM)
            {
                double x = Math.Min(pos.X, startPoint.X);
                double y = Math.Min(pos.Y, startPoint.Y);
                double w = Math.Abs(pos.X - startPoint.X);
                double h = Math.Abs(pos.Y - startPoint.Y);

                Canvas.SetLeft(pryam, x);
                Canvas.SetTop(pryam, y);
                pryam.Width = w;
                pryam.Height = h;
            }

            if (eleps_check && eleps != null && LKM)
            {
                double x = Math.Min(pos.X, startPoint.X);
                double y = Math.Min(pos.Y, startPoint.Y);
                double w = Math.Abs(pos.X - startPoint.X);
                double h = Math.Abs(pos.Y - startPoint.Y);

                Canvas.SetLeft(eleps, x);
                Canvas.SetTop(eleps, y);
                eleps.Width = w;
                eleps.Height = h;
            }

            if (treug_check && treug != null && LKM)
            {
                Point current = e.GetPosition(Miro);

                Point p1 = new Point(startPoint.X, current.Y);
                Point p2 = new Point((startPoint.X + current.X) / 2, startPoint.Y);
                Point p3 = new Point(current.X, current.Y);

                treug.Points = new PointCollection { p1, p2, p3 };
            }

        }

        private void SaveCanvasState()
        {
            int width = (int)Miro.ActualWidth;
            int height = (int)Miro.ActualHeight;
            if (width == 0 || height == 0) return;

            // Сохраняем старые параметры
            var oldTransform = Miro.LayoutTransform;
            var oldMargin = Miro.Margin;

            Miro.LayoutTransform = null;
            Miro.Margin = new Thickness(0);

            Miro.Measure(new Size(width, height));
            Miro.Arrange(new Rect(0, 0, width, height));

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(Miro);
            history.Push(rtb);

            Miro.LayoutTransform = oldTransform;
            Miro.Margin = oldMargin;
        }

        private void FloodFill(Canvas canvas, Point pt, Color fillColor)
        {
            SaveCanvasState();

            int width = (int)canvas.ActualWidth;
            int height = (int)canvas.ActualHeight;

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                VisualBrush vb = new VisualBrush(canvas);
                dc.DrawRectangle(vb, null, new Rect(0, 0, width, height));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            WriteableBitmap wb = new WriteableBitmap(rtb);

            int x = (int)pt.X;
            int y = (int)pt.Y;
            int stride = wb.PixelWidth * 4;
            byte[] pixels = new byte[wb.PixelHeight * stride];
            wb.CopyPixels(pixels, stride, 0);

            int startIndex = (y * stride) + x * 4;
            byte targetB = pixels[startIndex];
            byte targetG = pixels[startIndex + 1];
            byte targetR = pixels[startIndex + 2];

            if (targetR == fillColor.R && targetG == fillColor.G && targetB == fillColor.B)
                return;

            Queue<(int X, int Y)> q = new Queue<(int X, int Y)>();
            q.Enqueue((x, y));

            while (q.Count > 0)
            {
                var (cx, cy) = q.Dequeue();
                if (cx < 0 || cx >= wb.PixelWidth || cy < 0 || cy >= wb.PixelHeight)
                    continue;

                int index = (cy * stride) + cx * 4;

                byte b = pixels[index];
                byte g = pixels[index + 1];
                byte r = pixels[index + 2];

                if (Math.Abs(r - targetR) < 30 && Math.Abs(g - targetG) < 30 && Math.Abs(b - targetB) < 30)
                {
                    pixels[index] = fillColor.B;
                    pixels[index + 1] = fillColor.G;
                    pixels[index + 2] = fillColor.R;
                    pixels[index + 3] = 255;

                    q.Enqueue((cx + 1, cy));
                    q.Enqueue((cx - 1, cy));
                    q.Enqueue((cx, cy + 1));
                    q.Enqueue((cx, cy - 1));
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight), pixels, stride, 0);
            Image img = new Image { Source = wb };
            canvas.Children.Clear();
            canvas.Children.Add(img);
        }


        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
                button.ContextMenu.IsOpen = true;
        }

        private void Red_Click(object sender, RoutedEventArgs e) => cwet = Brushes.Red;
        private void Blue_Click(object sender, RoutedEventArgs e) => cwet = Brushes.Blue;
        private void Yellow_Click(object sender, RoutedEventArgs e) => cwet = Brushes.Yellow;
        private void Green_Click(object sender, RoutedEventArgs e) => cwet = Brushes.Green;
        private void Orange_Click(object sender, RoutedEventArgs e) => cwet = Brushes.Orange;
        private void Purple_Click(object sender, RoutedEventArgs e) => cwet = Brushes.Purple;
        private void Black_Click(object sender, RoutedEventArgs e) => cwet = Brushes.Black;
        private void White_Click(object sender, RoutedEventArgs e) => cwet = Brushes.White;
        private void Gray_Click(object sender, RoutedEventArgs e) => cwet = Brushes.Gray;
        private void Brown_Click(object sender, RoutedEventArgs e) => cwet = Brushes.Brown;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.Z)
            {
                if (history.Count > 0)
                {
                    var last = history.Pop();
                    Image img = new Image
                    {
                        Source = last,
                        Width = Miro.ActualWidth,
                        Height = Miro.ActualHeight,
                        Stretch = Stretch.None
                    };

                    Canvas.SetLeft(img, 0);
                    Canvas.SetTop(img, 0);

                    Miro.Children.Clear();
                    Miro.Children.Add(img);
                }
                else if (Miro.Children.Count > 0)
                {
                    Miro.Children.RemoveAt(Miro.Children.Count - 1);
                }
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C)
            {
                if(setka_check)
                {
                    for (int i = Miro.Children.Count - 1; i >= 0; i--)
                    {
                        if (Miro.Children[i] is Line line && line.Stroke == Brushes.LightGray)
                            Miro.Children.RemoveAt(i);
                    }
                }

                Copy();

                if (setka_check)
                {
                    double step = 10;
                    double width = Miro.ActualWidth;
                    double height = Miro.ActualHeight;

                    for (double y = 0; y < height; y += step)
                    {
                        Line line = new Line
                        {
                            X1 = 0,
                            Y1 = y,
                            X2 = width,
                            Y2 = y,
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 1,
                            IsHitTestVisible = false
                        };
                        Miro.Children.Add(line);
                    }

                    for (double x = 0; x < width; x += step)
                    {
                        Line line = new Line
                        {
                            X1 = x,
                            Y1 = 0,
                            X2 = x,
                            Y2 = height,
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 1,
                            IsHitTestVisible = false
                        };
                        Miro.Children.Add(line);
                    }
                }
                
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)
            {
                if (copiedImage != null)
                {
                    Cursor = Cursors.Cross;
                    Miro.MouseLeftButtonDown += vstavit;
                }
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.X)
            {

                if (setka_check)
                {
                    for (int i = Miro.Children.Count - 1; i >= 0; i--)
                    {
                        if (Miro.Children[i] is Line line && line.Stroke == Brushes.LightGray)
                            Miro.Children.RemoveAt(i);
                    }
                }

                Copy();
                Delite();

                if (setka_check)
                {
                    double step = 10;
                    double width = Miro.ActualWidth;
                    double height = Miro.ActualHeight;

                    for (double y = 0; y < height; y += step)
                    {
                        Line line = new Line
                        {
                            X1 = 0,
                            Y1 = y,
                            X2 = width,
                            Y2 = y,
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 1,
                            IsHitTestVisible = false
                        };
                        Miro.Children.Add(line);
                    }

                    for (double x = 0; x < width; x += step)
                    {
                        Line line = new Line
                        {
                            X1 = x,
                            Y1 = 0,
                            X2 = x,
                            Y2 = height,
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 1,
                            IsHitTestVisible = false
                        };
                        Miro.Children.Add(line);
                    }
                }
            }

            if (e.Key == Key.Delete)
            {
                Delite();
            }

            if (e.Key == Key.S)
            {
                vydel_check = true;
                instrument = "Выделение";
                if (instrumentText != null)
                    instrumentText.Text = $"Инструмент: — {instrument}";
                if (vydelRect != null)
                    if (vydelRect.Fill != Miro.Background)
                        Dell_vydel();

                karandash_check = false;
                lastik_check = false;
                zalivka_check = false;
                text_check = false;
                liniy_check = false;
                pryam_check = false;
                eleps_check = false;

                StopGlow();
            }

        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();

        private void StartGlow(Button btn)
        {
            StopGlow();
            activeButton = btn;
            timer.Interval = TimeSpan.FromMilliseconds(40);
            timer.Tick += (s, e) =>
            {
                if (activeButton == null) return;

                opacity += brighter ? 0.02 : -0.02;
                if (opacity >= 1) brighter = false;
                if (opacity <= 0.3) brighter = true;

                Color glow = Colors.DeepSkyBlue;
                glow.A = (byte)(opacity * 255);
                activeButton.Background = new SolidColorBrush(glow);
                activeButton.BorderBrush = Brushes.White;
            };
            timer.Start();
        }

        private void StopGlow()
        {
            timer.Stop();
            if (activeButton != null)
            {
                activeButton.Background = Brushes.Transparent;
                activeButton.BorderBrush = Brushes.Gray;
            }
            activeButton = null;
        }

        private void Karandash_Click(object sender, RoutedEventArgs e)
        {
            karandash_check = true;
            instrument = "Карандаш";
            if (instrumentText != null)
                instrumentText.Text = $"Инструмент: — {instrument}";

            if (vydelRect != null)
            if (vydelRect.Fill != Miro.Background)
                Dell_vydel();

            zalivka_check = false;
            lastik_check = false;
            text_check = false;
            liniy_check = false;
            pryam_check = false;
            eleps_check = false;
            treug_check = false;
            vydel_check = false;

            StopGlow();
        }

        private void Zalivka_Click(object sender, RoutedEventArgs e)
        {
            zalivka_check = true;
            instrument = "Заливка";
            if (instrumentText != null)
                instrumentText.Text = $"Инструмент: — {instrument}";

            if (vydelRect != null)
            if (vydelRect.Fill != Miro.Background)
                Dell_vydel();

            karandash_check = false;
            lastik_check = false;
            text_check = false;
            liniy_check = false;
            pryam_check = false;
            eleps_check = false;
            treug_check = false;
            vydel_check = false;

            StopGlow();
        }

        private void Lastik_Click(object sender, RoutedEventArgs e)
        {
            lastik_check = true;
            instrument = "Ластик";
            if (instrumentText != null)
                instrumentText.Text = $"Инструмент: — {instrument}";

            if (vydelRect != null)
            if (vydelRect.Fill != Miro.Background)
                Dell_vydel();

            zalivka_check = false;
            karandash_check = false;
            text_check = false;
            liniy_check = false;
            pryam_check = false;
            eleps_check = false;
            treug_check = false;
            vydel_check = false;

            StopGlow();
        }

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            text_check = true;
            instrument = "Текст";
            if (instrumentText != null)
                instrumentText.Text = $"Инструмент: — {instrument}";

            if (vydelRect != null)
            if (vydelRect.Fill != Miro.Background)
                Dell_vydel();

            lastik_check = false;
            zalivka_check = false;
            karandash_check = false;
            liniy_check = false;
            pryam_check = false;
            eleps_check = false;
            treug_check = false;
            vydel_check = false;

            StartGlow((Button)sender);
        }

        private void Liniy_Click(object sender, RoutedEventArgs e)
        {
            liniy_check = true;
            instrument = "Линия";
            if (instrumentText != null)
                instrumentText.Text = $"Инструмент: — {instrument}";

            if (vydelRect != null)
            if (vydelRect.Fill != Miro.Background)
                Dell_vydel();

            text_check = false;
            lastik_check = false;
            zalivka_check = false;
            karandash_check = false;
            pryam_check = false;
            eleps_check = false;
            treug_check = false;
            vydel_check = false;

            StartGlow((Button)sender);
        }

        private void Pryam_Click(object sender, RoutedEventArgs e)
        {
            pryam_check = true;
            instrument = "Прямоугольник";
            if (instrumentText != null)
                instrumentText.Text = $"Инструмент: — {instrument}";

            if (vydelRect != null)
            if (vydelRect.Fill != Miro.Background)
                Dell_vydel();

            liniy_check = false;
            karandash_check = false;
            lastik_check = false;
            zalivka_check = false;
            text_check = false;
            eleps_check = false;
            treug_check = false;
            vydel_check = false;

            StartGlow((Button)sender);
        }

        private void Eleps_Click(object sender, RoutedEventArgs e)
        {
            eleps_check = true;
            instrument = "Эллепс";
            if (instrumentText != null)
                instrumentText.Text = $"Инструмент: — {instrument}";

            if (vydelRect != null)
            if (vydelRect.Fill != Miro.Background)
                Dell_vydel();

            pryam_check = false;
            liniy_check = false;
            karandash_check = false;
            lastik_check = false;
            zalivka_check = false;
            text_check = false;
            treug_check = false;
            vydel_check = false;

            StartGlow((Button)sender);
        }

        private void Treug_Click(object sender, RoutedEventArgs e)
        {
            treug_check = true;
            instrument = "Треугольник";
            if (instrumentText != null)
                instrumentText.Text = $"Инструмент: — {instrument}";

            if (vydelRect != null)
            if (vydelRect.Fill != Miro.Background)
                Dell_vydel();

            eleps_check = false;
            pryam_check = false;
            liniy_check = false;
            karandash_check = false;
            lastik_check = false;
            zalivka_check = false;
            text_check = false;
            vydel_check = false;

            StartGlow((Button)sender);
        }
        private void vydel_Click(object sender, RoutedEventArgs e)
        {
            vydel_check = true;
            instrument = "Выделение";
            if (instrumentText != null)
                instrumentText.Text = $"Инструмент: — {instrument}";
            if (vydelRect != null)
            if (vydelRect.Fill != Miro.Background)
                Dell_vydel();

            karandash_check = false;
            lastik_check = false;
            zalivka_check = false;
            text_check = false;
            liniy_check = false;
            pryam_check = false;
            eleps_check = false;
            treug_check = false;

            StopGlow();
        }

        void DrawSetka()
        {

            if (!setka_check)
            {
                double step = 10;
                double width = Miro.ActualWidth;
                double height = Miro.ActualHeight;

                for (double y = 0; y < height; y += step)
                {
                    Line line = new Line
                    {
                        X1 = 0,
                        Y1 = y,
                        X2 = width,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };
                    Miro.Children.Add(line);
                }

                for (double x = 0; x < width; x += step)
                {
                    Line line = new Line
                    {
                        X1 = x,
                        Y1 = 0,
                        X2 = x,
                        Y2 = height,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };
                    Miro.Children.Add(line);
                }

                setka_check = true;
            }
            else
            {
                for (int i = Miro.Children.Count - 1; i >= 0; i--)
                {
                    if (Miro.Children[i] is Line line && line.Stroke == Brushes.LightGray)
                        Miro.Children.RemoveAt(i);
                }

                setka_check = false;
            }
        }

        void DrawSetkaChange()
        {
            if (setka_check)
            {
                for (int i = Miro.Children.Count - 1; i >= 0; i--)
                {
                    if (Miro.Children[i] is Line line && line.Stroke == Brushes.LightGray)
                    {
                        Miro.Children.RemoveAt(i);
                    }
                }
                double step = 10;
                double width = Miro.ActualWidth;
                double height = Miro.ActualHeight;

                for (double y = 0; y < height; y += step)
                {
                    Line line = new Line
                    {
                        X1 = 0,
                        Y1 = y,
                        X2 = width,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };
                    Miro.Children.Add(line);
                }

                for (double x = 0; x < width; x += step)
                {
                    Line line = new Line
                    {
                        X1 = x,
                        Y1 = 0,
                        X2 = x,
                        Y2 = height,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };
                    Miro.Children.Add(line);
                }
            }
        }

        private Border strokaSostoyaniya;
        private TextBlock mousePosText;
        private TextBlock instrumentText;
        private bool statusVisible = false;

        private void StrSost()
        {
            if (strokaSostoyaniya == null)
            {
                Grid parent = Miro.Parent as Grid;
                if (parent == null) return;

                int targetRow = 3;
                while (parent.RowDefinitions.Count <= targetRow)
                    parent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                strokaSostoyaniya = new Border
                {
                    Background = Brushes.LightGray,
                    Height = 25,
                    BorderBrush = Brushes.DarkGray,
                    BorderThickness = new Thickness(1)
                };

                DockPanel panel = new DockPanel();

                mousePosText = new TextBlock
                {
                    Text = "X: 0, Y: 0",
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                DockPanel.SetDock(mousePosText, Dock.Left);
                panel.Children.Add(mousePosText);

                instrumentText = new TextBlock
                {
                    Text = $"Инструмент: — {instrument}",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                panel.Children.Add(instrumentText);

                strokaSostoyaniya.Child = panel;

                parent.Children.Add(strokaSostoyaniya);
                Grid.SetRow(strokaSostoyaniya, targetRow);
            }

            statusVisible = !statusVisible;
            strokaSostoyaniya.Visibility = statusVisible ? Visibility.Visible : Visibility.Collapsed;

            Miro.UpdateLayout();

            Miro.MouseMove += (s, e) =>
            {
                if (!statusVisible) return;
                Point p = e.GetPosition(Miro);
                mousePosText.Text = $"X: {(int)p.X}, Y: {(int)p.Y}";
            };
        }

        private void Setka_Click(object sender, RoutedEventArgs e)
        {
            DrawSetka();
        }

        private void Str_sost_Click(object sender, RoutedEventArgs e)
        {
            StrSost();
        }

        private void Otmena_Click(object sender, RoutedEventArgs e)
        {
            if (history.Count > 0)
            {
                var last = history.Pop();
                Image img = new Image
                {
                    Source = last,
                    Width = Miro.ActualWidth,
                    Height = Miro.ActualHeight,
                    Stretch = Stretch.None
                };

                Canvas.SetLeft(img, 0);
                Canvas.SetTop(img, 0);

                Miro.Children.Clear();
                Miro.Children.Add(img);
            }
            else if (Miro.Children.Count > 0)
            {
                Miro.Children.RemoveAt(Miro.Children.Count - 1);
            }
        }

        private void Dell_vydel()
        {
            if (count_vydel == 1)
            {
                count_vydel--;
                for (int i = Miro.Children.Count - 1; i >= 0; i--)
                {
                    if (Miro.Children[i] is Rectangle vydelRect)
                    {
                        Miro.Children.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void Delite()
        {
            if (vydelRect == null || vydelRect.Width == 0 || vydelRect.Height == 0)
                return;

            vydelRect.Fill = Miro.Background;
            vydelRect.Stroke = Miro.Background;

            count_vydel--;
        }

        private void Copy()
        {
            if (vydelRect == null || vydelRect.Width == 0 || vydelRect.Height == 0)
                return;

            vydelRect.Stroke = Miro.Background;
            SaveCanvasState();

            double x = Canvas.GetLeft(vydelRect);
            double y = Canvas.GetTop(vydelRect);
            int width = (int)vydelRect.Width;
            int height = (int)vydelRect.Height;


            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)Miro.ActualWidth,
                (int)Miro.ActualHeight,
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(Miro);

            CroppedBitmap cb = new CroppedBitmap(rtb, new Int32Rect((int)x, (int)y, width, height));

            copiedImage = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawImage(cb, new Rect(0, 0, width, height));
            }
            copiedImage.Render(dv);
            vydelRect.Stroke = Brushes.Black;
        }

        private void vstavit(object sender, MouseButtonEventArgs e)
        {
            if (copiedImage == null) return;

            Point pos = e.GetPosition(Miro);

            Image img = new Image
            {
                Source = copiedImage,
                Width = copiedImage.PixelWidth,
                Height = copiedImage.PixelHeight
            };

            Canvas.SetLeft(img, pos.X);
            Canvas.SetTop(img, pos.Y);

            Miro.Children.Add(img);

            Miro.MouseLeftButtonDown -= vstavit;
            Cursor = Cursors.Arrow;
            if (setka_check)
                DrawSetkaChange();
        }

        private void Kopy_Click(object sender, RoutedEventArgs e)
        {
            if (setka_check)
            {
                for (int i = Miro.Children.Count - 1; i >= 0; i--)
                {
                    if (Miro.Children[i] is Line line && line.Stroke == Brushes.LightGray)
                        Miro.Children.RemoveAt(i);
                }
            }

            Copy();

            if (setka_check)
            {
                double step = 10;
                double width = Miro.ActualWidth;
                double height = Miro.ActualHeight;

                for (double y = 0; y < height; y += step)
                {
                    Line line = new Line
                    {
                        X1 = 0,
                        Y1 = y,
                        X2 = width,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };
                    Miro.Children.Add(line);
                }

                for (double x = 0; x < width; x += step)
                {
                    Line line = new Line
                    {
                        X1 = x,
                        Y1 = 0,
                        X2 = x,
                        Y2 = height,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };
                    Miro.Children.Add(line);
                }
            }
        }

        private void Vstavit_Click(object sender, RoutedEventArgs e)
        {
            if (copiedImage == null) return;

            Cursor = Cursors.Cross;
            Miro.MouseLeftButtonDown += vstavit;
        }

        private void Dell_Click(object sender, RoutedEventArgs e)
        {
            Delite();
        }

        private void Virezat_Click(object sender, RoutedEventArgs e)
        {
            if (setka_check)
            {
                for (int i = Miro.Children.Count - 1; i >= 0; i--)
                {
                    if (Miro.Children[i] is Line line && line.Stroke == Brushes.LightGray)
                        Miro.Children.RemoveAt(i);
                }
            }

            Copy();
            Delite();

            if (setka_check)
            {
                double step = 10;
                double width = Miro.ActualWidth;
                double height = Miro.ActualHeight;

                for (double y = 0; y < height; y += step)
                {
                    Line line = new Line
                    {
                        X1 = 0,
                        Y1 = y,
                        X2 = width,
                        Y2 = y,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };
                    Miro.Children.Add(line);
                }

                for (double x = 0; x < width; x += step)
                {
                    Line line = new Line
                    {
                        X1 = x,
                        Y1 = 0,
                        X2 = x,
                        Y2 = height,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1,
                        IsHitTestVisible = false
                    };
                    Miro.Children.Add(line);
                }
            }
        }

        private void obrezka()
        {
            if (vydelRect == null || vydelRect.Width <= 0 || vydelRect.Height <= 0)
                return;

            vydelRect.Stroke = Miro.Background;
            SaveCanvasState();

            double x = Canvas.GetLeft(vydelRect);
            double y = Canvas.GetTop(vydelRect);
            double width = vydelRect.Width;
            double height = vydelRect.Height;

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)Miro.ActualWidth, (int)Miro.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(Miro);

            Int32Rect cropRect = new Int32Rect((int)x, (int)y, (int)width, (int)height);
            CroppedBitmap cropped = new CroppedBitmap(rtb, cropRect);

            Miro.Children.Clear();

            Image img = new Image
            {
                Source = cropped,
                Width = Miro.ActualWidth,
                Height = Miro.ActualHeight,
                Stretch = Stretch.Fill
            };

            Canvas.SetLeft(img, 0);
            Canvas.SetTop(img, 0);
            Miro.Children.Add(img);

            vydelRect.Stroke = Brushes.Black;

            Dell_vydel();
        }


        private void Obrezka_Click(object sender, RoutedEventArgs e)
        {
            if (vydel_check)
                obrezka();
            else
                MessageBox.Show("Выдели область", "Спойлер");
        }

        private void povorot(double znachenie)
        {
            if (vydelRect == null || vydelRect.Width <= 0 || vydelRect.Height <= 0)
                return;

            vydelRect.Stroke = Miro.Background;
            SaveCanvasState();

            double x = Canvas.GetLeft(vydelRect);
            double y = Canvas.GetTop(vydelRect);
            double width = vydelRect.Width;
            double height = vydelRect.Height;

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)Miro.ActualWidth, (int)Miro.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(Miro);

            Int32Rect cropRect = new Int32Rect((int)x, (int)y, (int)width, (int)height);
            CroppedBitmap cropped = new CroppedBitmap(rtb, cropRect);

            int newWidth = (znachenie % 180 == 0) ? (int)width : (int)height;
            int newHeight = (znachenie % 180 == 0) ? (int)height : (int)width;

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.PushTransform(new TranslateTransform(newWidth / 2, newHeight / 2));
                dc.PushTransform(new RotateTransform(znachenie));
                dc.PushTransform(new TranslateTransform(-width / 2, -height / 2));

                dc.DrawImage(cropped, new Rect(0, 0, width, height));

                dc.Pop(); dc.Pop(); dc.Pop();
            }

            RenderTargetBitmap rotated = new RenderTargetBitmap(newWidth, newHeight, 96, 96, PixelFormats.Pbgra32);
            rotated.Render(dv);

            Delite();

            Image img = new Image
            {
                Source = rotated,
                Width = newWidth,
                Height = newHeight
            };

            double newX = x + (width - newWidth) / 2;
            double newY = y + (height - newHeight) / 2;

            Canvas.SetLeft(img, newX);
            Canvas.SetTop(img, newY);
            Miro.Children.Add(img);


            Dell_vydel();
        }

        private double ShowInputDialog(string text, string caption)
        {
            Window inputDialog = new Window
            {
                Width = 300,
                Height = 150,
                Title = caption,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(10) };

            TextBlock message = new TextBlock { Text = text, Margin = new Thickness(0, 0, 0, 10) };
            TextBox inputBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            Button okButton = new Button { Content = "OK", Width = 60, IsDefault = true, HorizontalAlignment = HorizontalAlignment.Right };
            okButton.Click += (_, __) => inputDialog.DialogResult = true;

            panel.Children.Add(message);
            panel.Children.Add(inputBox);
            panel.Children.Add(okButton);
            inputDialog.Content = panel;

            if (inputDialog.ShowDialog() == true)
                return Convert.ToDouble(inputBox.Text);
            else
                return 0;
        }

        private void Povorot_Click(object sender, RoutedEventArgs e)
        {
            if(vydel_check)
                povorot(ShowInputDialog("Введите значение угла", "Значение"));
            else
                MessageBox.Show("Выдели область", "Спойлер");
        }

        private void save()
        {
            try
            {
                SaveCanvasState();

                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    (int)Miro.ActualWidth,
                    (int)Miro.ActualHeight,
                    96, 96,
                    PixelFormats.Pbgra32);

                rtb.Render(Miro);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string fileName = $"Рисунок_{DateTime.Now:HH-mm-ss}.png";
                string fullPath = System.IO.Path.Combine(desktop, fileName);

                using (FileStream fs = new FileStream(fullPath, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                MessageBox.Show($"Изображение сохранено на рабочем столе:\n{fileName}", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            save();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "PNG Files|*.png|JPEG Files|*.jpg;*.jpeg|All Files|*.*";

            bool? result = dlg.ShowDialog();
            if (result != true) return;

            string filePath = dlg.FileName;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            Image img = new Image
            {
                Source = bitmap,
                Width = bitmap.PixelWidth,
                Height = bitmap.PixelHeight
            };

            Canvas.SetLeft(img, 0);
            Canvas.SetTop(img, 0);

            Miro.Children.Add(img);
        }
    }
}
