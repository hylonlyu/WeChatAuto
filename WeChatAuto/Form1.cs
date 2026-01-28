using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeChatAuto
{
    /// <summary>
    /// 联系人项目类，包含姓名和金额信息
    /// </summary>


    public partial class Form1 : Form
    {
        // 联系人列表数据文件路径
        private static readonly string ContactsDataFile = Path.Combine(Application.StartupPath, "contacts.json");

        // TCP相关常量和字段
        private const int DefaultPort = 9000;
        private const int MaxLogEntries = 500;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private bool isConnecting = false;
        private bool autoReconnect = true;
        private System.Timers.Timer reconnectTimer;

        // 心跳相关字段
        private System.Timers.Timer heartbeatTimer;
        private bool isWaitingForHeartbeatAck = false;
        private DateTime lastHeartbeatSentTime;
        private int currentHeartbeatInterval = 5000; // 当前心跳间隔（毫秒），默认5秒
        private const int HeartbeatTimeout = 5000;   // 5秒心跳超时
        private const int DefaultHeartbeatInterval = 5; // 默认心跳间隔（秒）
        private const int MinHeartbeatInterval = 3;   // 最小心跳间隔（秒）
        private const int MaxHeartbeatInterval = 60;  // 最大心跳间隔（秒）

        // 联系人列表数据
        private List<ContactItem> contacts = new List<ContactItem>();

        // Windows API声明
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        // 鼠标事件常量
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        // 常量定义
        private const int SW_RESTORE = 9;
        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;

        public Form1()
        {
            InitializeComponent();

            // 手动绑定lstName的双击事件
            if (lstName != null)
            {
                lstName.DoubleClick += lstName_DoubleClick;
            }

            // 手动绑定btnConnect的点击事件
            if (btnConnect != null)
            {
                btnConnect.Click += btnConnect_Click;
            }

            // 手动绑定心跳间隔的失去焦点事件
            if (txtHeartbeat != null)
            {
                txtHeartbeat.Leave += txtHeartbeat_Leave;
                txtHeartbeat.Validating += txtHeartbeat_Validating;
            }

            LoadContactsFromFile(); // 启动时加载联系人列表和服务器配置
            UpdateConnectionStatus("未连接", Color.Red); // 初始化连接状态显示
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxAdd.Text) && string.IsNullOrWhiteSpace(txtMoney.Text))
            {
                MessageBox.Show("姓名和金额不能同时为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = textBoxAdd.Text ?? string.Empty;
            decimal amount = 10; // 默认值

            if (!string.IsNullOrWhiteSpace(txtMoney.Text))
            {
                if (!decimal.TryParse(txtMoney.Text, out amount) || amount <= 0)
                {
                    MessageBox.Show("金额必须是大于0的数字，已设置为默认值10！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    amount = 10;
                    txtMoney.Text = "10";
                }
            }

            // 创建新的联系人项目
            var contact = new ContactItem(name, amount);
            contacts.Add(contact);

            // 更新ListView显示
            UpdateListView();

            // 清空输入框
            textBoxAdd.Clear();
            txtMoney.Clear();

            SaveContactsToFile(); // 添加联系人后保存
        }

        private void buttonOut_Click(object sender, EventArgs e)
        {
            if (lstName.SelectedItems.Count > 0)
            {
                var selectedItem = lstName.SelectedItems[0];
                string contactInfo = $"{selectedItem.SubItems[0].Text} - {selectedItem.SubItems[1].Text}";

                var result = MessageBox.Show($"确定要删除联系人 '{contactInfo}' 吗？", "确认删除",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // 从数据列表中删除对应的联系人
                    int index = selectedItem.Index;
                    if (index >= 0 && index < contacts.Count)
                    {
                        contacts.RemoveAt(index);
                        UpdateListView();
                        SaveContactsToFile(); // 移除联系人后保存
                    }
                }
            }
            else
            {
                MessageBox.Show("请先选择要删除的联系人！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // 禁用按钮防止重复点击
            button1.Enabled = false;

            try
            {
                bool allSuccess = await SendMsg2Contact(txtMsg.Text);

                if (allSuccess)
                {
                    // 将焦点转移回当前应用程序
                    this.Activate();
                    this.Focus();
                    // MessageBox.Show("消息发送完成！");
                }
            }
            finally
            {
                // 重新启用按钮
                button1.Enabled = true;
            }
        }

        private async Task<bool> SendMsg2Contact(string msg)
        {
            bool allSuccess = true;

            // 直接在UI线程中执行消息发送，确保剪贴板操作在STA模式下进行
            for (int i = 0; i < contacts.Count; i++)
            {
                bool success = SendWeChatMessage(msg, contacts[i].Name, contacts[i].Amount);
                if (!success)
                {
                    allSuccess = false;
                }

                // 在每个消息发送后短暂延迟，避免操作过快
                await Task.Delay(100);
            }

            return allSuccess;
        }

        public bool SendWeChatMessage(string result, string name, decimal amount)
        {
            Console.WriteLine("微信自动化消息发送演示 - 使用Windows API");
            Console.WriteLine("========================================");

            try
            {
                // 检查微信是否已运行
                if (!IsWeChatRunning())
                {
                    Console.WriteLine("请先启动微信并登录！");
                    Console.WriteLine("\n按任意键退出...");
                    Console.ReadKey();
                    return false;
                }

                // 获取要发送消息的联系人名称和消息内容
                string contactName = name;

                // 处理 result 内容：按回车键分割，将 amount 附加到每行后面
                string[] lines = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                List<string> processedLines = new List<string>();

                foreach (string line in lines)
                {
                    // 如果不是空行，将 amount 附加到行内容后面
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string amountStr = ((int)amount).ToString(); // 转为整数，不保留小数
                        if(line.Contains("第") && line.Contains("场"))
                        {
                           // 特殊处理包含“第”和“场”的行，不添加金额
                            processedLines.Add(line);
                        }
                        else
                        {
                            processedLines.Add(line + amountStr);
                        }
                    }
                    else
                    {
                        // 空行直接忽略，不添加到结果中
                        continue;
                    }
                }

                // 判断处理结果，如果没有内容（比如原result为空），则直接使用原result加上amount
                string messageContent;
                if (processedLines.Count == 0 && !string.IsNullOrEmpty(result))
                {
                    // 如果原result不为空但处理后为空（可能是因为特殊字符或空行），直接加上amount
                    messageContent = result + ((int)amount).ToString();
                }
                else if (processedLines.Count == 0)
                {
                    // 如果原result为空，则直接为空字符串
                    messageContent = result;
                }
                else
                {
                    // 将处理后的行用回车键重新连接成消息内容
                    messageContent = string.Join("\r\n", processedLines);
                }

                // 发送消息
                bool success = SendMessageToWeChatContact(contactName, messageContent);

                if (success)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("消息发送失败！");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                MessageBox.Show($"发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        private static bool IsWeChatRunning()
        {
            return Process.GetProcessesByName("WeiXin").Any();
        }

        /// <summary>
        /// 使用Windows API向微信联系人发送消息
        /// </summary>
        /// <param name="contactName">联系人昵称</param>
        /// <param name="message">消息内容</param>
        /// <returns>是否发送成功</returns>
        private static bool SendMessageToWeChatContact(string contactName, string message)
        {
            if (string.IsNullOrEmpty(contactName) || string.IsNullOrEmpty(message))
            {
                Console.WriteLine("联系人名称或消息内容不能为空！");
                return false;
            }

            try
            {
                // 获取微信进程
                var processes = Process.GetProcessesByName("WeiXin");
                if (processes.Length == 0)
                {
                    Console.WriteLine("未找到微信进程！");
                    return false;
                }

                // 获取微信窗口并激活
                IntPtr wechatWindowHandle = IntPtr.Zero;

                // 方法1: 通过进程主窗口句柄获取
                foreach (var process in processes)
                {
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        wechatWindowHandle = process.MainWindowHandle;
                        break;
                    }
                }

                // 方法2: 如果方法1失败，尝试通过窗口标题查找
                if (wechatWindowHandle == IntPtr.Zero)
                {
                    // 微信窗口标题通常包含"微信"或"WeChat"
                    wechatWindowHandle = FindWindow(null, "微信");
                    if (wechatWindowHandle == IntPtr.Zero)
                    {
                        wechatWindowHandle = FindWindow(null, "WeChat");
                    }
                }

                if (wechatWindowHandle == IntPtr.Zero)
                {
                    Console.WriteLine("无法获取微信窗口句柄！");
                    return false;
                }

                // 显示窗口并设置为前台
                ShowWindow(wechatWindowHandle, SW_RESTORE);
                SetForegroundWindow(wechatWindowHandle);
                Thread.Sleep(100); // 等待窗口激活

                Console.WriteLine($"正在查找联系人 '{contactName}'...");

                // 使用Ctrl+F打开搜索框
                SimulateKeyDown(VK_CONTROL);
                SimulateKeyDown(VK_F);
                SimulateKeyUp(VK_F);
                SimulateKeyUp(VK_CONTROL);
                Thread.Sleep(50);

                // 输入联系人名称
                Console.WriteLine("输入联系人名称...");
                TypeText(contactName);
                Thread.Sleep(50);

                // 按Enter选择联系人
                SimulateKeyDown(VK_RETURN);
                SimulateKeyUp(VK_RETURN);
                Thread.Sleep(100);

                // 使用鼠标点击输入框位置，确保获得焦点
                Console.WriteLine("点击输入框以获取焦点...");
                ClickInputArea();
                Thread.Sleep(50);

                // 输入消息内容
                Console.WriteLine("输入消息内容...");
                TypeText(message);
                Thread.Sleep(100);

                // 按Enter发送消息
                SimulateKeyDown(VK_RETURN);
                SimulateKeyUp(VK_RETURN);
                Thread.Sleep(50);
                SimulateKeyDown(VK_RETURN);
                SimulateKeyUp(VK_RETURN);
                Thread.Sleep(50);
                /**/
                Console.WriteLine("消息发送完成！");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"自动化操作失败: {ex.Message}");
                return false;
            }
        }

        // 虚拟键码常量
        private const byte VK_CONTROL = 0x11;
        private const byte VK_F = 0x46;
        private const byte VK_RETURN = 0x0D;
        private const byte VK_V = 0x56;

        /// <summary>
        /// 模拟键盘按下和释放
        /// </summary>
        private static void SimulateKeyDown(byte key)
        {
            keybd_event(key, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        }

        private static void SimulateKeyUp(byte key)
        {
            keybd_event(key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        /// <summary>
        /// 智能计算并点击微信消息输入区域
        /// 根据窗口尺寸和位置动态计算输入框位置，提高准确性
        /// </summary>
        private static void ClickInputArea()
        {
            // 获取当前活动窗口的信息
            RECT windowRect = new RECT();
            IntPtr foregroundWindow = GetForegroundWindow();

            if (GetWindowRect(foregroundWindow, ref windowRect))
            {
                // 计算窗口的宽度和高度
                int width = windowRect.right - windowRect.left;
                int height = windowRect.bottom - windowRect.top;

                Console.WriteLine($"检测到微信窗口尺寸: 宽度={width}, 高度={height}");
                Console.WriteLine($"窗口位置: 左={windowRect.left}, 上={windowRect.top}, 右={windowRect.right}, 下={windowRect.bottom}");

                // 智能计算输入框位置
                // 1. 水平位置：窗口宽度的中间位置（略偏左一些，因为输入框通常不是完全居中）
                int x = windowRect.left + (int)(width * 0.4); // 稍微偏左，更符合输入框实际位置

                // 2. 垂直位置：考虑不同窗口尺寸的输入框位置
                //    - 对于标准微信窗口，输入框通常在底部上方60-80像素
                //    - 动态计算，根据窗口高度调整
                int inputBoxOffset = 70; // 默认偏移量
                if (height < 600)        // 小窗口模式
                {
                    inputBoxOffset = 60;
                }
                else if (height > 800)   // 大窗口模式
                {
                    inputBoxOffset = 80;
                }

                int y = windowRect.bottom - inputBoxOffset;

                Console.WriteLine($"计算的输入框位置: X={x}, Y={y}");

                // 移动鼠标到输入框位置并点击
                Console.WriteLine("正在移动鼠标到输入框位置...");
                SetCursorPos(x, y);
                Thread.Sleep(200); // 给鼠标移动的时间

                Console.WriteLine("点击输入框...");
                SimulateMouseClick();
            }
            else
            {
                // 如果无法获取窗口位置，使用自适应默认位置
                Console.WriteLine("无法获取窗口信息，使用自适应默认位置点击...");

                // 获取屏幕工作区大小，计算更适合的默认位置
                int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

                // 计算默认位置为屏幕中心偏下
                int defaultX = screenWidth / 2;
                int defaultY = screenHeight - 100;

                Console.WriteLine($"使用屏幕中心偏下位置: X={defaultX}, Y={defaultY}");
                SetCursorPos(defaultX, defaultY);
                Thread.Sleep(200);
                SimulateMouseClick();
            }
        }

        /// <summary>
        /// 模拟鼠标左键点击
        /// </summary>
        private static void SimulateMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(50); // 按下后短暂延迟
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }

        // Windows API用于获取窗口信息
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        // 窗口矩形结构
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        /// <summary>
        /// 模拟文本输入（使用剪贴板和Ctrl+V粘贴方式）
        /// </summary>
        private static void TypeText(string text)
        {
            Console.WriteLine($"正在准备文本粘贴: {text}");

            // 保存当前剪贴板内容
            IDataObject originalClipboard = System.Windows.Forms.Clipboard.GetDataObject();

            try

            {
                // 将文本复制到剪贴板
                System.Windows.Forms.Clipboard.SetText(text);
                Thread.Sleep(100); // 等待剪贴板操作完成

                // 发送Ctrl+V组合键粘贴文本
                SimulateKeyDown(VK_CONTROL);
                SimulateKeyDown(VK_V);
                SimulateKeyUp(VK_V);
                SimulateKeyUp(VK_CONTROL);
                Thread.Sleep(500); // 给足够的时间处理粘贴操作
            }
            catch (Exception ex)
            {
                MessageBox.Show($"粘贴文本时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"粘贴文本时出错: {ex.Message}");
            }
            finally
            {
                // 恢复原始剪贴板内容（如果有的话）
                if (originalClipboard != null)
                {
                    try
                    {
                        System.Windows.Forms.Clipboard.SetDataObject(originalClipboard, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"恢复剪贴板内容时出错: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 保存联系人列表和端口配置到JSON文件
        /// </summary>
        private void SaveContactsToFile()
        {
            try
            {
                var contactsData = new List<object>();
                foreach (var contact in contacts)
                {
                    contactsData.Add(new { name = contact.Name, amount = contact.Amount });
                }

                var jsonData = new
                {
                    contacts = contactsData,
                    serverConfig = new
                    {
                        serverIP = txtServerIP?.Text ?? "127.0.0.1",
                        serverPort = txtServerPort?.Text ?? "9000",
                        heartbeatInterval = GetHeartbeatIntervalFromUI()
                    },
                    lastSaved = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(jsonData);
                File.WriteAllText(ContactsDataFile, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从JSON文件加载联系人列表和端口配置
        /// </summary>
        private void LoadContactsFromFile()
        {
            try
            {
                if (File.Exists(ContactsDataFile))
                {
                    string json = File.ReadAllText(ContactsDataFile, Encoding.UTF8);
                    var serializer = new JavaScriptSerializer();
                    var jsonData = serializer.Deserialize<Dictionary<string, object>>(json);

                    if (jsonData != null)
                    {
                        contacts.Clear(); // 清空现有联系人列表

                        // 加载联系人列表
                        if (jsonData.ContainsKey("contacts"))
                        {
                            var contactsData = jsonData["contacts"] as ArrayList;
                            if (contactsData != null)
                            {
                                // 检查是否为新格式（包含name和amount的对象）
                                bool isNewFormat = false;
                                if (contactsData.Count > 0)
                                {
                                    var firstContact = contactsData[0] as Dictionary<string, object>;
                                    if (firstContact != null && firstContact.ContainsKey("name"))
                                    {
                                        isNewFormat = true;
                                    }
                                }

                                if (isNewFormat)
                                {
                                    // 新格式：包含name和amount的对象
                                    foreach (var contact in contactsData)
                                    {
                                        var contactDict = contact as Dictionary<string, object>;
                                        if (contactDict != null)
                                        {
                                            string name = contactDict.ContainsKey("name") ? contactDict["name"].ToString() : string.Empty;
                                            decimal amount = 10; // 默认金额

                                            if (contactDict.ContainsKey("amount"))
                                            {
                                                decimal.TryParse(contactDict["amount"].ToString(), out amount);
                                                if (amount <= 0) amount = 10; // 确保金额大于0
                                            }

                                            contacts.Add(new ContactItem(name, amount));
                                        }
                                    }
                                }
                                else
                                {
                                    // 旧格式：只有姓名的字符串列表，为旧联系人设置默认金额10
                                    foreach (var contact in contactsData)
                                    {
                                        if (contact != null)
                                        {
                                            string name = contact.ToString();
                                            contacts.Add(new ContactItem(name, 10)); // 默认金额10
                                        }
                                    }
                                }
                            }
                        }

                        // 更新ListView显示
                        UpdateListView();

                        // 加载服务器配置
                        if (jsonData.ContainsKey("serverConfig"))
                        {
                            var serverConfig = jsonData["serverConfig"] as Dictionary<string, object>;
                            if (serverConfig != null)
                            {
                                if (serverConfig.ContainsKey("serverIP"))
                                {
                                    txtServerIP.Text = serverConfig["serverIP"].ToString();
                                }
                                if (serverConfig.ContainsKey("serverPort"))
                                {
                                    txtServerPort.Text = serverConfig["serverPort"].ToString();
                                }
                                if (serverConfig.ContainsKey("heartbeatInterval"))
                                {
                                    int loadedInterval = DefaultHeartbeatInterval;
                                    if (int.TryParse(serverConfig["heartbeatInterval"].ToString(), out loadedInterval))
                                    {
                                        SetHeartbeatIntervalToUI(loadedInterval);
                                    }
                                }
                                else
                                {
                                    // 兼容旧配置，设置默认值
                                    SetHeartbeatIntervalToUI(DefaultHeartbeatInterval);
                                }
                            }
                        }
                        else if (jsonData.ContainsKey("port")) // 兼容旧的UDP配置
                        {
                            int savedPort = Convert.ToInt32(jsonData["port"]);
                            if (IsValidPort(savedPort))
                            {
                                txtServerPort.Text = savedPort.ToString();
                            }
                        }
                    }
                }
                else
                {
                    // 使用默认配置
                    txtServerIP.Text = "127.0.0.1";
                    txtServerPort.Text = DefaultPort.ToString();
                    SetHeartbeatIntervalToUI(DefaultHeartbeatInterval);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置失败，使用默认设置: {ex.Message}");
                txtServerIP.Text = "127.0.0.1";
                txtServerPort.Text = DefaultPort.ToString();
                SetHeartbeatIntervalToUI(DefaultHeartbeatInterval);
                contacts.Clear();
                UpdateListView();
            }
        }

        /// <summary>
        /// 验证端口号是否有效
        /// </summary>
        private bool IsValidPort(int port)
        {
            return port > 0 && port <= 65535;
        }

        /// <summary>
        /// 验证端口号是否有效
        /// </summary>
        private bool IsValidPort(string portText)
        {
            if (int.TryParse(portText, out int port))
            {
                return IsValidPort(port);
            }
            return false;
        }

        /// <summary>
        /// 更新连接状态显示
        /// </summary>
        private void UpdateConnectionStatus(string status, Color color)
        {
            if (lblConnectionStatus.InvokeRequired)
            {
                lblConnectionStatus.Invoke(new Action(() =>
                {
                    lblConnectionStatus.Text = status;
                    lblConnectionStatus.ForeColor = color;
                }));
            }
            else
            {
                lblConnectionStatus.Text = status;
                lblConnectionStatus.ForeColor = color;
            }
        }


        /// <summary>
        /// 添加日志消息到lstInfo
        /// </summary>
        private void AddLogMessage(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AddLogMessage), message);
                return;
            }

            try
            {
                string timestamp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
                string logEntry = $"{timestamp} {message}";

                lstInfo.Items.Add(logEntry);

                // 限制日志条数，超过则移除旧的
                while (lstInfo.Items.Count > MaxLogEntries)
                {
                    lstInfo.Items.RemoveAt(0);
                }

                // 自动滚动到最新消息
                lstInfo.TopIndex = lstInfo.Items.Count - 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加日志消息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 添加网络日志消息到lstNet
        /// </summary>
        private void AddNetLogMessage(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AddNetLogMessage), message);
                return;
            }

            try
            {
                string timestamp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
                string logEntry = $"{timestamp} {message}";

                lstNet.Items.Add(logEntry);

                // 限制日志条数，超过则移除旧的
                while (lstNet.Items.Count > MaxLogEntries)
                {
                    lstNet.Items.RemoveAt(0);
                }

                // 自动滚动到最新消息
                lstNet.TopIndex = lstNet.Items.Count - 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加网络日志消息失败: {ex.Message}");
            }
        }

        
        /// <summary>
        /// 更新ListView显示
        /// </summary>
        private void UpdateListView()
        {
            lstName.Items.Clear();
            foreach (var contact in contacts)
            {
                var item = new ListViewItem(new string[] { contact.Name, contact.Amount.ToString() });
                lstName.Items.Add(item);
            }
        }

        /// <summary>
        /// ListView双击事件处理 - 编辑金额
        /// </summary>
        private void lstName_DoubleClick(object sender, EventArgs e)
        {
            if (lstName.SelectedItems.Count > 0)
            {
                var selectedItem = lstName.SelectedItems[0];
                int index = selectedItem.Index;

                if (index >= 0 && index < contacts.Count)
                {
                    // 检查是否点击了第二列（金额列）
                    Rectangle rect = lstName.GetItemRect(selectedItem.Index);
                    int columnX = lstName.Columns[0].Width;

                    if (MousePosition.X - lstName.PointToScreen(rect.Location).X > columnX)
                    {
                        // 双击了金额列，进行编辑
                        EditAmount(selectedItem, index);
                    }
                }
            }
        }

        /// <summary>
        /// 编辑联系人金额
        /// </summary>
        /// <param name="listViewItem">ListView项</param>
        /// <param name="contactIndex">联系人索引</param>
        private void EditAmount(ListViewItem listViewItem, int contactIndex)
        {
            var currentAmount = contacts[contactIndex].Amount;

            // 创建简单的输入对话框
            Form inputDialog = new Form();
            inputDialog.Text = $"编辑 '{contacts[contactIndex].Name}' 的金额";
            inputDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputDialog.StartPosition = FormStartPosition.CenterParent;
            inputDialog.MaximizeBox = false;
            inputDialog.MinimizeBox = false;
            inputDialog.Width = 300;
            inputDialog.Height = 150;

            Label label = new Label();
            label.Text = "请输入新的金额：";
            label.Left = 20;
            label.Top = 20;
            label.Width = 200;

            TextBox textBox = new TextBox();
            textBox.Text = currentAmount.ToString();
            textBox.Left = 20;
            textBox.Top = 40;
            textBox.Width = 200;

            Button okButton = new Button();
            okButton.Text = "确定";
            okButton.DialogResult = DialogResult.OK;
            okButton.Left = 20;
            okButton.Top = 80;
            okButton.Width = 80;

            Button cancelButton = new Button();
            cancelButton.Text = "取消";
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Left = 110;
            cancelButton.Top = 80;
            cancelButton.Width = 80;

            inputDialog.Controls.Add(label);
            inputDialog.Controls.Add(textBox);
            inputDialog.Controls.Add(okButton);
            inputDialog.Controls.Add(cancelButton);
            inputDialog.AcceptButton = okButton;
            inputDialog.CancelButton = cancelButton;

            if (inputDialog.ShowDialog() == DialogResult.OK)
            {
                SaveEditedAmount(textBox.Text, contactIndex, listViewItem);
            }
        }

        /// <summary>
        /// 保存编辑后的金额
        /// </summary>
        /// <param name="newAmountText">新金额文本</param>
        /// <param name="contactIndex">联系人索引</param>
        /// <param name="listViewItem">ListView项</param>
        private void SaveEditedAmount(string newAmountText, int contactIndex, ListViewItem listViewItem)
        {
            decimal newAmount;
            if (decimal.TryParse(newAmountText, out newAmount) && newAmount > 0)
            {
                contacts[contactIndex].Amount = newAmount;
                listViewItem.SubItems[1].Text = newAmount.ToString();
                SaveContactsToFile();
            }
            else
            {
                MessageBox.Show("金额必须是大于0的数字！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 连接按钮点击事件
        /// </summary>
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (tcpClient == null || !tcpClient.Connected)
            {
                // 连接服务器
                if (!ValidateInput())
                    return;

                int port = int.Parse(txtServerPort.Text);
                bool success = await ConnectToServer(txtServerIP.Text, port);

                if (success)
                {
                    btnConnect.Text = "断开";
                    autoReconnect = true;
                }
            }
            else
            {
                // 断开连接
                DisconnectFromServer();
                btnConnect.Text = "连接";
            }
        }

        /// <summary>
        /// 验证输入有效性
        /// </summary>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtServerIP.Text))
            {
                MessageBox.Show("服务器IP不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!IsValidPort(txtServerPort.Text))
            {
                MessageBox.Show($"端口 '{txtServerPort.Text}' 无效！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        private async Task<bool> ConnectToServer(string ip, int port)
        {
            try
            {
                UpdateConnectionStatus("连接中...", Color.Orange);
                isConnecting = true;

                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(ip, port);

                networkStream = tcpClient.GetStream();
                UpdateConnectionStatus("已连接", Color.Green);

                // 开始接收数据
                StartTcpReceive();

                // 连接成功后启动心跳机制
                StartHeartbeat();

                return true;
            }
            catch (Exception ex)
            {
                UpdateConnectionStatus("连接失败", Color.Red);
                AddNetLogMessage($"连接失败: {ex.Message}");
                return false;
            }
            finally
            {
                isConnecting = false;
            }
        }

        /// <summary>
        /// 开始接收TCP数据
        /// </summary>
        private async void StartTcpReceive()
        {
            try
            {
                var buffer = new byte[8192];

                while (tcpClient.Connected && networkStream != null)
                {
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        // 服务器断开连接
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // 复用现有的消息处理逻辑
                    ProcessReceivedMessage(message);
                }
            }
            catch (Exception ex)
            {
                if (tcpClient.Connected)
                {
                    AddNetLogMessage($"接收数据异常: {ex.Message}");
                }
            }
            finally
            {
                HandleConnectionLost();
            }
        }

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        private void ProcessReceivedMessage(string message)
        {
            // 检查是否为心跳响应
            if (IsHeartbeatAck(message))
            {
                HandleHeartbeatAck();
                return;
            }

            AddLogMessage(message);
            // 将接收到的消息设置到消息框并自动发送
            this.Invoke(new Action(() =>
            {
                txtMsg.Text = message;
                button1.PerformClick();
            }));
        }

        /// <summary>
        /// 处理连接丢失
        /// </summary>
        private void HandleConnectionLost()
        {
            UpdateConnectionStatus("连接断开", Color.Red);
            AddNetLogMessage("与服务器的连接已断开");

            // 停止心跳机制
            StopHeartbeat();

            // 清理资源
            networkStream?.Close();
            tcpClient?.Close();
            networkStream = null;
            tcpClient = null;

            // 自动重连
            if (autoReconnect)
            {
                StartAutoReconnect();
            }
        }

        /// <summary>
        /// 开始自动重连
        /// </summary>
        private void StartAutoReconnect()
        {
            reconnectTimer = new System.Timers.Timer(5000); // 5秒重连一次
            reconnectTimer.Elapsed += async (sender, e) => {
                if (!isConnecting && (tcpClient == null || !tcpClient.Connected))
                {
                    UpdateConnectionStatus("重连中...", Color.Orange);
                    int port = int.Parse(txtServerPort.Text);
                    bool success = await ConnectToServer(txtServerIP.Text, port);

                    if (success)
                    {
                        reconnectTimer.Stop();
                    }
                }
            };
            reconnectTimer.AutoReset = true;
            reconnectTimer.Start();
        }

        /// <summary>
        /// 断开与服务器的连接
        /// </summary>
        private void DisconnectFromServer()
        {
            try
            {
                autoReconnect = false; // 停止自动重连
                reconnectTimer?.Stop();

                // 停止心跳机制
                StopHeartbeat();

                networkStream?.Close();
                tcpClient?.Close();

                UpdateConnectionStatus("未连接", Color.Red);
                AddNetLogMessage("已断开连接");
            }
            catch (Exception ex)
            {
                AddNetLogMessage($"断开连接异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 窗体关闭时清理资源
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveContactsToFile();
            DisconnectFromServer(); // 断开TCP连接
        }

        #region 心跳机制相关方法

        /// <summary>
        /// 启动心跳定时器
        /// </summary>
        private void StartHeartbeat()
        {
            try
            {
                StopHeartbeat(); // 先停止现有的心跳定时器

                // 从UI获取当前心跳间隔
                int interval = GetHeartbeatIntervalFromUI();
                currentHeartbeatInterval = interval * 1000; // 转换为毫秒

                heartbeatTimer = new System.Timers.Timer(currentHeartbeatInterval);
                heartbeatTimer.Elapsed += OnHeartbeatTimerElapsed;
                heartbeatTimer.AutoReset = true;
                heartbeatTimer.Start();

                AddNetLogMessage($"心跳机制已启动，间隔: {interval}秒");
            }
            catch (Exception ex)
            {
                AddNetLogMessage($"启动心跳机制失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止心跳定时器
        /// </summary>
        private void StopHeartbeat()
        {
            try
            {
                if (heartbeatTimer != null)
                {
                    heartbeatTimer.Stop();
                    heartbeatTimer.Elapsed -= OnHeartbeatTimerElapsed;
                    heartbeatTimer.Dispose();
                    heartbeatTimer = null;
                }

                isWaitingForHeartbeatAck = false;
                AddNetLogMessage("心跳机制已停止");
            }
            catch (Exception ex)
            {
                AddNetLogMessage($"停止心跳机制失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 心跳定时器事件处理
        /// </summary>
        private void OnHeartbeatTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // 检查是否还在等待上一次的心跳响应
                if (isWaitingForHeartbeatAck)
                {
                    // 检查是否超时
                    if (DateTime.Now - lastHeartbeatSentTime > TimeSpan.FromMilliseconds(HeartbeatTimeout))
                    {
                        // 心跳超时，判定连接断开
                        AddNetLogMessage("心跳超时，连接已断开");
                        isWaitingForHeartbeatAck = false;

                        // 在UI线程中处理连接断开
                        this.Invoke(new Action(() =>
                        {
                            HandleConnectionLost();
                        }));
                        return;
                    }
                    // 还在等待，跳过本次心跳
                    return;
                }

                // 发送心跳
                SendHeartbeat();
            }
            catch (Exception ex)
            {
                AddNetLogMessage($"心跳处理异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        private void SendHeartbeat()
        {
            try
            {
                if (tcpClient != null && tcpClient.Connected && networkStream != null)
                {
                    byte[] heartbeatData = Encoding.UTF8.GetBytes("HEARTBEAT");
                    networkStream.Write(heartbeatData, 0, heartbeatData.Length);

                    lastHeartbeatSentTime = DateTime.Now;
                    isWaitingForHeartbeatAck = true;

                    AddNetLogMessage("发送心跳包");
                }
                else
                {
                    AddNetLogMessage("TCP连接不可用，无法发送心跳");
                }
            }
            catch (Exception ex)
            {
                AddNetLogMessage($"发送心跳包失败: {ex.Message}");
                // 发送失败也判定为连接断开
                this.Invoke(new Action(() =>
                {
                    HandleConnectionLost();
                }));
            }
        }

        /// <summary>
        /// 检查是否为心跳响应
        /// </summary>
        /// <param name="message">接收到的消息</param>
        /// <returns>是否为心跳响应</returns>
        private bool IsHeartbeatAck(string message)
        {
            return message == "HEARTBEAT_ACK";
        }

        /// <summary>
        /// 处理心跳响应
        /// </summary>
        private void HandleHeartbeatAck()
        {
            if (isWaitingForHeartbeatAck)
            {
                isWaitingForHeartbeatAck = false;
                AddNetLogMessage("收到心跳响应");
            }
        }

        #endregion

        #region 心跳间隔配置相关方法

        /// <summary>
        /// 从UI获取心跳间隔（秒）
        /// </summary>
        private int GetHeartbeatIntervalFromUI()
        {
            if (int.TryParse(txtHeartbeat?.Text, out int interval))
            {
                // 确保在有效范围内
                if (interval < MinHeartbeatInterval) return MinHeartbeatInterval;
                if (interval > MaxHeartbeatInterval) return MaxHeartbeatInterval;
                return interval;
            }
            return DefaultHeartbeatInterval;
        }

        /// <summary>
        /// 设置心跳间隔到UI
        /// </summary>
        private void SetHeartbeatIntervalToUI(int interval)
        {
            // 确保在有效范围内
            if (interval < MinHeartbeatInterval) interval = MinHeartbeatInterval;
            if (interval > MaxHeartbeatInterval) interval = MaxHeartbeatInterval;

            if (txtHeartbeat != null)
            {
                txtHeartbeat.Text = interval.ToString();
            }
            // 同时更新当前心跳间隔
            currentHeartbeatInterval = interval * 1000; // 转换为毫秒
        }

        /// <summary>
        /// 验证心跳间隔输入
        /// </summary>
        private bool IsValidHeartbeatInterval(string intervalText)
        {
            if (!int.TryParse(intervalText, out int interval))
            {
                return false;
            }

            return interval >= MinHeartbeatInterval && interval <= MaxHeartbeatInterval;
        }

        /// <summary>
        /// 心跳间隔输入框失去焦点事件
        /// </summary>
        private void txtHeartbeat_Leave(object sender, EventArgs e)
        {
            ValidateHeartbeatInput();
        }

        /// <summary>
        /// 心跳间隔输入框验证事件
        /// </summary>
        private void txtHeartbeat_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ValidateHeartbeatInput())
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 验证心跳输入并应用
        /// </summary>
        private bool ValidateHeartbeatInput()
        {
            if (string.IsNullOrWhiteSpace(txtHeartbeat?.Text))
            {
                MessageBox.Show($"心跳间隔不能为空，请输入{MinHeartbeatInterval}-{MaxHeartbeatInterval}之间的数字",
                    "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetHeartbeatIntervalToUI(DefaultHeartbeatInterval);
                return false;
            }

            if (!IsValidHeartbeatInterval(txtHeartbeat.Text))
            {
                MessageBox.Show($"心跳间隔必须是{MinHeartbeatInterval}-{MaxHeartbeatInterval}秒之间的数字",
                    "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetHeartbeatIntervalToUI(DefaultHeartbeatInterval);
                return false;
            }

            // 应用新的心跳间隔
            int newInterval = GetHeartbeatIntervalFromUI();
            if (newInterval * 1000 != currentHeartbeatInterval)
            {
                currentHeartbeatInterval = newInterval * 1000; // 转换为毫秒
                AddNetLogMessage($"心跳间隔已更新为: {newInterval}秒");

                // 如果心跳定时器正在运行，重新启动以应用新间隔
                if (heartbeatTimer != null)
                {
                    RestartHeartbeatWithNewInterval();
                }
            }

            return true;
        }

        /// <summary>
        /// 使用新的间隔重启心跳定时器
        /// </summary>
        private void RestartHeartbeatWithNewInterval()
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                int newInterval = currentHeartbeatInterval / 1000; // 转换为秒
                AddNetLogMessage($"应用新的心跳间隔: {newInterval}秒");

                // 停止当前心跳
                StopHeartbeat();

                // 重新启动心跳（会使用新的间隔）
                StartHeartbeat();
            }
        }

        #endregion

        private void lblHeartbeat_Click(object sender, EventArgs e)
        {

        }
    }

    public class ContactItem
    {
        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">姓名</param>
        /// <param name="amount">金额，默认为10</param>
        public ContactItem(string name, decimal amount = 10)
        {
            Name = name ?? string.Empty;
            Amount = amount > 0 ? amount : 10; // 确保金额大于0
        }

        /// <summary>
        /// 重写ToString方法，用于显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name} - {Amount}";
        }
    }
}
