using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace HuffmanDecoder
{
    public partial class MainWindow : Window
    {
        private TreeNode treeRoot; //保存树的根节点
        private bool codeShown = false; //用于显示/隐藏编码的扩展区的标记
        Dictionary<string, char> codeMap = new Dictionary<string, char>(); //编码查询字典

        public MainWindow()
        {
            InitializeComponent();
            Dictionary<char, int> data = null;
            try
            {
                data = ReadData("Resources/data.csv");
            }
            catch (Exception ex)
            {
                MessageBox.Show("对不起，程序出现错误，错误信息如下：\n" + ex.Message, 
                    "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            treeRoot = InitTree(data);
            StringBuilder sb = new StringBuilder();
            var keys = codeMap.Keys.ToList();
            for (int i = 0; i < (keys.Count + 1) / 2; i++) //按列排列，即第一行为_N，第二行为AO，以此类推
            {
                string key = keys[i];
                char value = codeMap[keys[i]] == ' ' ? '_' : codeMap[keys[i]];
                string entry = $"{key,15}={value,-2}";//编码预留15字符，右对齐；明文预留2字符，左对齐

                int oppositeIndex = i + keys.Count / 2 + 1; //本行第二列的文本信息
                if (oppositeIndex < keys.Count) 
                {
                    key = keys[oppositeIndex];
                    value = codeMap[keys[oppositeIndex]] == ' ' ? '_' : codeMap[keys[oppositeIndex]];
                    entry += $"{key,15}={value,-2}";
                }
                sb.Append(entry + "\n");
            }
            lbl_CodeInfo.Text = sb.ToString(); //显示在前端
        }

        //读取csv文件
        private Dictionary<char, int> ReadData(string path) 
        {
            Dictionary<char, int> data = new Dictionary<char, int>();
            StreamReader file = File.OpenText(path);
            while (file != null && !file.EndOfStream)
            {
                string line = file.ReadLine(); //按行读取文件
                char key = line[0];
                int weight = Int32.Parse(line.Substring(2));
                data.Add(key, weight);
            }
            file.Close();
            return data;
        }

        //初始化树
        private TreeNode InitTree(Dictionary<char, int> data)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            List<TreeNode> leafNodes = new List<TreeNode>();
            foreach (KeyValuePair<char, int> pair in data)//将有字符数据的叶子结点提前抽出
            {
                TreeNode leafNode = new TreeNode(pair.Key, pair.Value);
                nodes.Add(leafNode);
                leafNodes.Add(leafNode);
            }
            while (nodes.Count > 1)//反复选出权值最小的两个结点
            {
                TreeNode left = PopNodeWithLeastWeight(nodes);
                TreeNode right = PopNodeWithLeastWeight(nodes);
                nodes.Add(TreeNode.CombineNode(left, right));
            }
            AllocatePathCode(nodes[0]);
            foreach (TreeNode node in leafNodes)
            {
                codeMap.Add(node.PathCode, node.Value);
            }
            if (nodes.Count == 1)
            {
                return nodes[0];
            }
            return null;
        }
        //弹出最小权值结点
        private TreeNode PopNodeWithLeastWeight(List<TreeNode> list)
        {
            if (list.Count == 0)
            {
                return null;
            }
            TreeNode nodeToPop = list[0];
            int min = list[0].Weight;
            foreach (TreeNode node in list)
            {
                if (node.Weight < min)
                {
                    min = node.Weight;
                    nodeToPop = node;
                }
            }
            list.Remove(nodeToPop);
            return nodeToPop;
        }
        //以先序遍历的方式为生成好的树分配结点编码
        private void AllocatePathCode(TreeNode root, string path = "")
        {
            if (root == null)
            {
                return;
            }
            root.PathCode = path;
            AllocatePathCode(root.LeftChild, path + "0");
            AllocatePathCode(root.RightChild, path + "1");
        }
        //显示哈夫曼树
        private void btn_ShowHuffmanTree_Click(object sender, RoutedEventArgs e)
        {
            HuffmanWindow huffmanWindow = new HuffmanWindow(treeRoot);
            huffmanWindow.Owner = this;
            huffmanWindow.Show();
        }
        //译码：遇0往左，遇1往右，遇叶子结点则译出一位
        private void btn_Decode_Click(object sender, RoutedEventArgs e)
        {
            tb_DecodeResult.Text = "";
            string source = tb_InputCode.Text;
            TreeNode nodePtr = treeRoot;
            int sourcePos = 0;
            while (sourcePos < source.Length)
            {
                int startIndex = sourcePos;
                while (!nodePtr.IsLeaf)
                {
                    if (sourcePos >= source.Length)
                    {
                        MessageBox.Show($"在编码尾部发现非法码：{source.Substring(startIndex)}"
                            , "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (source[sourcePos] == '0')
                    {
                        nodePtr = nodePtr.LeftChild;
                    }
                    else if (source[sourcePos] == '1')
                    {
                        nodePtr = nodePtr.RightChild;
                    }
                    else
                    {
                        MessageBox.Show($"发现非法字符：{source[sourcePos]}", "错误",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    sourcePos++;
                }
                tb_DecodeResult.Text += nodePtr.Value;
                nodePtr = treeRoot;
            }
        }
        //关于
        private void btn_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("版权所有：计算机科学与技术211班 张健\n日期：2023年9月", "关于程序");
        }
        //回车快捷键
        private void tb_InputCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btn_Decode_Click(sender, e);
            }
        }

        //展开/折叠编码信息区
        private void btn_ShowCode_Click(object sender, RoutedEventArgs e)
        {
            if (codeShown)
            {
                CodeRow.Height = new GridLength(0);
                codeShown = false;
                btn_ShowCode.Content = "显示编码";
            }
            else
            {
                CodeRow.Height = new GridLength(300);
                codeShown = true;
                btn_ShowCode.Content = "隐藏编码";
            }
        }
    }
    //树结点类
    public class TreeNode
    {
        public bool IsLeaf { get; set; }
        public TreeNode Parent { get; set; }
        public TreeNode LeftChild { get; set; }
        public TreeNode RightChild { get; set; }
        public char Value { get; } //字符
        public int Weight { get; } //权值
        public string PathCode { get; set; } = ""; //储存编码

        public TreeNode(char value, int weight)
        {
            Value = value;
            Weight = weight;
            IsLeaf = true;
        }
        private TreeNode(int weight)
        {
            Weight = weight;
            IsLeaf = false;
        }
        public static TreeNode CombineNode(TreeNode left, TreeNode right) //静态方法，用于给定两个子结点生成新的父结点
        {
            TreeNode result = new TreeNode(left.Weight + right.Weight);
            result.LeftChild = left;
            result.RightChild = right;
            left.Parent = result;
            right.Parent = result;
            return result;
        }
        public override string ToString() //用于调试，将结点转化为字符串
        {
            return "[" + (IsLeaf ? Value + "," : "") + Weight + "]";
        }
    }
}
