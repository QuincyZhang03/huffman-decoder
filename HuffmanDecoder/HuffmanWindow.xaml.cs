using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HuffmanDecoder
{
    public partial class HuffmanWindow : Window
    {
        private const int NODE_SIZE = 40; //结点圆大小
        private const int VERTICAL_GAP = 70; //层间距
        private const int FONT_SIZE = 15; //字体大小
        private TreeNode root; //根节点
        private HuffmanWindow()
        {
            InitializeComponent();
        }
        private void SetNodeStyle(Ellipse node, string toolTip) //设置结点圆样式
        {
            node.Height = NODE_SIZE;
            node.Width = NODE_SIZE;
            node.Stroke = Brushes.Black;
            node.StrokeThickness = 1;
            node.Fill = Brushes.LightGreen;
            Label l = new Label();
            l.Content = toolTip;
            node.ToolTip = l;
            ToolTipService.SetInitialShowDelay(node, 0); //即时显示悬停提示
        }
        public HuffmanWindow(TreeNode root) : this() //公开构造方法，需传入根节点以绘制树
        {
            this.root = root;
            DrawNode(root, 0, new Point(canvas.Width / 2, 10 + NODE_SIZE / 2));
        }
        //绘制结点的方法，参数1为结点信息，参数2为结点层数，参数3为结点圆位置。该方法被递归调用，以先序遍历的方式绘制哈夫曼树。
        private void DrawNode(TreeNode node, int tier, Point pos)
        {
            Ellipse nodeElps = new Ellipse();
            SetNodeStyle(nodeElps,
                (node.IsLeaf ? (node.Value == ' ' ? "空格" : node.Value + "") + "，对应哈夫曼编码为" + node.PathCode : "非叶子结点")
                + "\n权值为：" + node.Weight + "\n");
            if (node.IsLeaf) //叶子结点，有码
            {
                nodeElps.Fill = Brushes.Blue;
                Label val = new Label();
                val.Content = node.Value;
                val.FontSize = FONT_SIZE;
                val.Foreground = Brushes.White;
                val.FontWeight = FontWeights.Bold;
                val.HorizontalContentAlignment = HorizontalAlignment.Left;
                val.VerticalContentAlignment = VerticalAlignment.Top;
                Canvas.SetLeft(val, pos.X - FONT_SIZE * 0.7);
                Canvas.SetTop(val, pos.Y - FONT_SIZE);
                Canvas.SetZIndex(val, 1);
                canvas.Children.Add(val);
            }
            Canvas.SetLeft(nodeElps, pos.X - NODE_SIZE / 2);
            Canvas.SetTop(nodeElps, pos.Y - NODE_SIZE / 2);
            canvas.Children.Add(nodeElps);
            if (node.LeftChild != null)
            {
                double x2 = pos.X - canvas.Width / Math.Pow(2, tier + 2);
                double y2 = pos.Y + VERTICAL_GAP;
                double r = NODE_SIZE / 2;
                double theta = Math.Atan2(y2 - pos.Y, pos.X - x2);//注意y轴是向下的
                Point lineP1 = new Point(pos.X - r * Math.Cos(theta), pos.Y + r * Math.Sin(theta));
                Point lineP2 = new Point(x2 + r * Math.Cos(theta), y2 - r * Math.Sin(theta));
                Line conjunction = new Line();
                conjunction.X1 = lineP1.X;
                conjunction.Y1 = lineP1.Y;
                conjunction.X2 = lineP2.X;
                conjunction.Y2 = lineP2.Y;
                conjunction.Stroke = Brushes.Black;
                conjunction.StrokeThickness = 2;

                canvas.Children.Add(conjunction);

                Point leftPos = new Point(x2, y2);
                //向左走的步长是该层横向间距的一半
                DrawNode(node.LeftChild, tier + 1, leftPos);//画左孩子
            }
            if (node.RightChild != null)
            {
                double x2 = pos.X + canvas.Width / Math.Pow(2, tier + 2);
                double y2 = pos.Y + VERTICAL_GAP;
                double r = NODE_SIZE / 2;
                double theta = Math.Atan2(y2 - pos.Y, x2 - pos.X);//注意y轴是向下的
                Point lineP1 = new Point(pos.X + r * Math.Cos(theta), pos.Y + r * Math.Sin(theta));
                Point lineP2 = new Point(x2 - r * Math.Cos(theta), y2 - r * Math.Sin(theta));
                Line conjunction = new Line();
                conjunction.X1 = lineP1.X;
                conjunction.Y1 = lineP1.Y;
                conjunction.X2 = lineP2.X;
                conjunction.Y2 = lineP2.Y;
                conjunction.Stroke = Brushes.Black;
                conjunction.StrokeThickness = 2;

                canvas.Children.Add(conjunction);

                Point rightPos = new Point(x2, y2);
                DrawNode(node.RightChild, tier + 1, rightPos);//画右孩子
            }
        }
        //鼠标滚轮滚动缩放功能
        private void scrollPane_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            const double SCALING_FACTOR = 0.5;//滑动一个鼠标滚轮单位的缩放量
            double alteredWidth = canvas.Width * (1 + SCALING_FACTOR * e.Delta / Math.Abs(e.Delta));
            if (alteredWidth < 1000)
            {
                alteredWidth = 1000;
            }
            double ratio = alteredWidth / canvas.Width;
            canvas.Width = alteredWidth;

            canvas.Children.Clear();
            DrawNode(root, 0, new Point(canvas.Width / 2, 10 + NODE_SIZE / 2));//重画哈夫曼树
            scrollPane.ScrollToHorizontalOffset(e.GetPosition(canvas).X * ratio - e.GetPosition(scrollPane).X);
            //滚动屏幕，保持鼠标所指向的位置在屏幕中的位置不变
        }
    }

}