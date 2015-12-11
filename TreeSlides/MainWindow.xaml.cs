using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using IOPath = System.IO.Path;

namespace TreeSlides
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Node tree;

        Node currentNode;



        double inactiveVsActiveScale = 1;
        double nestingShiftFraction = 0.1;
        double lineFraction = 0.1;

        Size? canvasSize;

        TimeSpan transitionDuration = TimeSpan.FromSeconds(0.4);

        public MainWindow()
        {
            InitializeComponent();

            LoadBackground();

            SnapsToDevicePixels = true;

            Loaded += (o, e) => Load();
        }

        void LoadBackground()
        {
            var path = IOPath.Combine(IOPath.GetDirectoryName(Assembly.GetEntryAssembly().Location), "bg.png");
            image.Source = new BitmapImage(new Uri(path));

        }

        void ToggleFullScreen()
        {
            if (this.WindowStyle == WindowStyle.None)
            {
                this.WindowStyle = WindowStyle.ThreeDBorderWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        void Load()
        {
            presentationCanvas.Children.Clear();
            canvasSize = null;

            LoadTree();

            int displayIndex = 0;
            Action<Node, Node, int> add = null;
            Node previous = null;

            add = (n, parent, level) =>
            {
                n.ContentUI.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
                n.ContentUI.Opacity = 0;

                n.Parent = parent;
                n.Index = displayIndex++;

                n.Previous = previous;
                if (previous != null)
                    previous.Next = n;

                previous = n;

                n.Level = level;

                var ui = n.ContentUI;

                presentationCanvas.Children.Add(ui);

                var trans = new MatrixTransform(Matrix.Identity);
                ui.RenderTransform = trans;

                for (int i = 0; i < n.Nodes.Count; i++)
                {
                    var child = n.Nodes[i];
                    add(child, n, level + 1);
                }
            };

            add(tree, null, 0);

            ActivateNode(tree);
        }

        void LoadTree()
        {
            var filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(GetType().Assembly.Location), "topics.xml");
            var x = XDocument.Load(new FileStream(filename, FileMode.Open));

            var list = new List<Node>();

            Action<List<Node>, XElement> cellReader = null;

            cellReader = (collection, cell) =>
             {
                 var textNode = cell.Nodes().FirstOrDefault(n => n is XText) as XText;
                 string text = textNode == null ? null : textNode.Value?.Trim().Replace("\\n", Environment.NewLine).Replace("\\t", "  ");

                 var node = new Node(text);
                 collection.Add(node);

                 var innerGrid = cell.Element("grid");
                 if (innerGrid != null)
                 {
                     foreach (var row in innerGrid.Elements("row"))
                     {
                         var firstCell = row.Element("cell");
                         if (firstCell != null)
                         {
                             cellReader(node.Nodes, firstCell);
                         }
                     }

                 }

             };

            cellReader(list, x.Element("cell"));

            tree = new Node("Topics") { Nodes = list[0].Nodes };
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            Node nextNode = null;
            switch (e.Key)
            {
                case Key.Right:
                    nextNode = currentNode.Next;
                    break;
                case Key.Left:
                    nextNode = currentNode.Previous;
                    break;
                case Key.Escape:
                    nextNode = currentNode.Parent;
                    break;
                case Key.Up:
                    {
                        var parent = currentNode.Parent;
                        if (parent != null)
                        {
                            var i = parent.Nodes.IndexOf(currentNode) - 1;
                            nextNode = i >= 0 ? parent.Nodes[i] : null;
                        }
                    }
                    break;
                case Key.Down:
                    {
                        var parent = currentNode.Parent;
                        if (parent != null)
                        {
                            var i = parent.Nodes.IndexOf(currentNode) + 1;
                            if (i < parent.Nodes.Count)
                                nextNode = parent.Nodes[i];
                        }
                    }
                    break;
                case Key.Home:
                    nextNode = tree;
                    break;
                case Key.F11:
                    ToggleFullScreen();
                    break;
                case Key.F5:
                    Load();
                    break;
            }

            if (nextNode != null)
                ActivateNode(nextNode);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (currentNode != null)
                ActivateNode(currentNode);
        }

        Size GetCanvasSize()
        {
            var size = presentationCanvas.RenderSize;
            var width = size.Width;
            var height = size.Height;

            var canvasSize = new Size(width * 0.7, height * 0.7);

            if (canvasSize != this.canvasSize)
            {
                //remeasure
                var fontSize = canvasSize.Height * lineFraction;

                var node = tree;
                while (node != null)
                {
                    node.ContentUI.SetValue(TextBlock.FontSizeProperty, fontSize);
                    node.ContentUI.SetValue(FrameworkElement.MaxWidthProperty, canvasSize.Width);
                    node.ContentUI.Measure(canvasSize);


                    node = node.Next;
                }

                this.canvasSize = canvasSize;
            }


            return canvasSize;
        }

        void ActivateNode(Node newNode)
        {
            currentNode = newNode;

            var canvasSize = GetCanvasSize();

            var ancestors = new HashSet<Node>() { newNode };
            var parent = newNode.Parent;

            while (parent != null)
            {
                ancestors.Add(parent);
                parent = parent.Parent;
            }

            Dictionary<Node, double?> yPositions = new Dictionary<Node, double?>(presentationCanvas.Children.Count);
            Action<Node, bool> yDeterminer = null;
            double y = 0;
            bool folding = false;
            yDeterminer = (n, visible) =>
              {
                  if (!visible)
                  {
                      folding |= (double)n.ContentUI.GetValue(UIElement.OpacityProperty) > 0;
                      yPositions[n] = null;
                  }
                  else
                  {
                      yPositions[n] = y;
                      y += n.ContentUI.DesiredSize.Height + lineFraction * canvasSize.Height;
                  }

                  foreach (var child in n.Nodes)
                  {
                      var childVisible = visible && ancestors.Contains(n);
                      yDeterminer(child, childVisible);
                  }
              };

            yDeterminer(tree, true);

            var containerSize = presentationCanvas.RenderSize;
            var canvasLeft = (containerSize.Width - canvasSize.Width) / 2;
            var canvasYCenter = containerSize.Height / 2;

            var offset = yPositions[newNode].Value + newNode.ContentUI.DesiredSize.Height / 2 - canvasYCenter;
            var transitDelay = folding ? TimeSpan.FromSeconds(0.3) : TimeSpan.Zero;

            var node = tree;
            while (node != null)
            {
                var position = yPositions[node];
                if (position == null)
                {
                    ChangeElementOpacity(node.ContentUI, 0);
                }
                else
                {
                    var isSiblingOrChild = node.Parent == newNode || node.Parent == newNode.Parent;

                    var opacity = node == newNode ? 1 : (node.Index > newNode.Index && !isSiblingOrChild ? 0.2 : 0.4);
                    var opacityDelay = transitDelay;
                    if ((double)node.ContentUI.GetValue(UIElement.OpacityProperty) == 0)
                        opacityDelay += TimeSpan.FromSeconds(0.4);

                    ChangeElementOpacity(node.ContentUI, opacity, opacityDelay);

                    var x = canvasLeft + (node.Level - newNode.Level) * canvasSize.Width * nestingShiftFraction;
                    var elementY = position.Value - offset;

                    var to = CalcTransform(node.ContentUI, x, elementY, inactiveVsActiveScale);

                    if ((node.ContentUI.RenderTransform as MatrixTransform).Matrix == Matrix.Identity)
                        (node.ContentUI.RenderTransform as MatrixTransform).Matrix = to;

                    TransitElement(node.ContentUI, to, transitDelay);
                }

                node = node.Next;
            }

        }

        void ChangeElementOpacity(UIElement element, double opacity, TimeSpan delay = default(TimeSpan))
        {
            element.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(opacity, TimeSpan.FromSeconds(0.3)) { BeginTime = delay });
        }

        void TransitElement(UIElement element, Matrix to, TimeSpan delay, Matrix? from = null)
        {
            var anim = new MatrixAnimation(from, to, transitionDuration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            anim.BeginTime = delay;

            element.RenderTransform.BeginAnimation(MatrixTransform.MatrixProperty, anim);
        }

        Matrix CalcTransform(UIElement element, double leftX, double topY, double scale)
        {
            var measured = element.DesiredSize;
            if (measured.Width == 0 || measured.Height == 0)
                return Matrix.Identity;

            var result = Matrix.Identity;

            result.Scale(scale, scale);
            result.Translate(leftX, topY);

            return result;
        }
    }


    class Node
    {
        public object Content;
        public UIElement ContentUI;

        public List<Node> Nodes = new List<Node>();

        public Node Parent;
        public Node Next;
        public Node Previous;
        public int Level;
        public int Index;

        public Node() { }

        public Node(string content)
        {
            Content = content;
            ContentUI = new TextBlock { Text = content, Foreground = Brushes.White };
        }

        public override string ToString()
        {
            return (Content ?? "").ToString();
        }
    }
}

