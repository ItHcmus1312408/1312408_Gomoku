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
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;


namespace CaroGomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Create_ArrayBtn();
            CreateArr();
            //PrintMessage();
        }

        private int count = 0;//xác định lượt đi
        private int[][] arr = new int[12][];//mảng lưu trữ nước đi
        private Button[,] Btn_Arr;//bàn cờ
        private int mode = 0; //chọn chế độ
        string name;//tên đăng kí chơi Online
        int r_rec = -1;//lưu vị trí dòng nhận đc khi chơi Onl
        int c_rec = -1;//lưu vị trí cột nhận đc khi chơi Onl
        int player = -1;//xác định lượt đi khi Onl
        int turn = -1;//xác định lượt đi khi Onl
        public Socket s;
        bool flag = false;//cờ đánh dấu On/Off
        string ms;//message nhận đc
        bool check_step = true;//kiểm tra tính hợp lệ của nước đi
        Ellipse ell = new Ellipse();//hiển thị nước cờ
        bool getConnected = false;//cờ báo kết nối online
        string name_player;//thông tin tin nhắn nhận đc
        int result_game = 0;//xét thắng/thua
        int sendMsg = 0;//cờ báo gửi tin nhắn
        bool getPos = true;//kiểm tra nước đi của máy
        bool EndPlay = false;//kiểm tra kết thúc
        bool changeName = false;//cờ thông báo đổi tên
        string pre_name = "";//đánh dấu mỗi lần đổi tên
        string namePlayer;//lưu tên người chơi
        bool start;//đánh dấu bắt đầu trò chơi
        bool checkName = true;//kiểm tra tên đăng kí
        private void Create_ArrayBtn() //tạo bàn cờ
        {
            Btn_Arr = new Button[12, 12];
            for (int i = 0; i < 12; i++)
            {
                ell.Height = 30;
                ell.Width = 30;
                ell.Fill = System.Windows.Media.Brushes.SandyBrown;
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                main_panel.Children.Add(panel);
                for (int j = 0; j < 12; j++)
                {
                    Button btn = new Button();
                    if (j % 2 == 0)

                        btn.Background = System.Windows.Media.Brushes.Lavender;

                    else

                        btn.Background = System.Windows.Media.Brushes.LightGray;


                    btn.Height = 40;
                    btn.Width = 40;
                    btn.Name = "Btn" + i.ToString() + j.ToString();
                    btn.Tag = string.Format("[{0},{1}]", i, j);
                    if (mode != 4)
                    {
                        if (flag == false)
                            btn.Click += new RoutedEventHandler(Btn_Click);
                        else
                            btn.Click += new RoutedEventHandler(Btn_Click_Onl);
                    }
                   
                    panel.Children.Add(btn);
                    Btn_Arr[i, j] = btn;
                    if (i % 2 == 1)
                    {
                        if (j % 2 == 0)
                        {
                            btn.Background = System.Windows.Media.Brushes.LightGray;
                        }
                        else

                            btn.Background = System.Windows.Media.Brushes.Lavender;
                    }
                }
            }
        }
        private void CreateArr()//tạo mảng lưu nước đi
        {
            for (int i = 0; i < 12; i++)
            {
                arr[i] = new int[12];
                for (int j = 0; j < 12; j++)
                    arr[i][j] = 0;
            }
        }
        private int Check_Winner(int rw, int cl)//kiểm tra thắng
        {
            bool Player1, Player2;
            int r = 0, c = 0;
            int i;

            //Kiem tra tren dong...
            while (c < 8)
            {
                Player1 = true; Player2 = true;

                for (i = 0; i < 5; i++)
                {
                    if (arr[rw][c + i] != 1)
                        Player1 = false;
                    if (arr[rw][c + i] != 2)
                        Player2 = false;
                }

                if (Player1 && (arr[rw][c] != 2 || arr[rw][c + 5] != 2)) return 1;
                if (Player2 && (arr[rw][c] != 1 || arr[rw][c + 5] != 1)) return 2;
                c++;
            }

            // Kiem tra tren cot...
            while (r < 8)
            {
                Player1 = true; Player2 = true;
                for (i = 0; i < 5; i++)
                {
                    if (arr[r + i][cl] != 1)
                        Player1 = false;
                    if (arr[r + i][cl] != 2)
                        Player2 = false;
                }
                if (Player1 && (arr[r][cl] != 2 || arr[r + 5][cl] != 2)) return 1;
                if (Player2 && (arr[r][cl] != 1 || arr[r + 5][cl] != 1)) return 2;
                r++;
            }

            // Kiem tra tren duong cheo xuong.
            r = rw; c = cl;
            // Di chuyen den dau duong cheo xuong.
            while (r > 0 && c > 0) { r--; c--; }
            while (r < 8 && c < 8)
            {
                Player1 = true; Player2 = true;
                for (i = 0; i < 5; i++)
                {
                    if (arr[r + i][c + i] != 1)
                        Player1 = false;
                    if (arr[r + i][c + i] != 2)
                        Player2 = false;
                }
                if (Player1 && (arr[r][c] != 2 || arr[r + 5][c + 5] != 2)) return 1;
                if (Player2 && (arr[r][c] != 1 || arr[r + 5][c + 5] != 1)) return 2;
                r++; c++;
            }

            // Kiem tra duong cheo len...
            r = rw; c = cl;
            // Di chuyen den dau duong cheo len...
            while (r < 11 && c > 0) { r++; c--; }
            while (r >= 4 && c < 8)
            {
                Player1 = true; Player2 = true;
                for (i = 0; i < 5; i++)
                {
                    if (arr[r - i][c + i] != 1)
                        Player1 = false;
                    if (arr[r - i][c + i] != 2)
                        Player2 = false;
                }
                if (Player1 && (arr[r][c] != 2 || arr[r - 4][c + 4] != 2)) return 1;
                if (Player2 && (arr[r][c] != 1 || arr[r - 4][c + 4] != 1)) return 2;
                r--; c++;
            }

            return 0;
        }
        private void Check_result(int ro, int co)//hiển thị kết quả
        {
            result_game = Check_Winner(ro, co);
            if (result_game == 0)
                return;

            if(result_game == 1)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    flag = false;
                    PP.IsEnabled = true;
                    pc.IsEnabled = true;
                    pc_Off.IsEnabled = true;
                    PP_Off.IsEnabled = true;
                   
                    string message = name + " won the game!";
                    PrintMessage(message, "Server");
                    getPos = false;
                }));
               
            }
            if(result_game == 2)
            {
                flag = false;                
                this.Dispatcher.Invoke((Action)(() =>
                {
                    PP.IsEnabled = true;
                    pc.IsEnabled = true;
                    pc_Off.IsEnabled = true;
                    PP_Off.IsEnabled = true;
                    getPos = false;
                    PrintMessage(namePlayer + " won the game!", "Server");
                }));
            }

            string msg = "Bạn có muốn bắt đầu trò chơi mới không?";
            MessageBoxResult option = MessageBox.Show(msg, "Gomoku", MessageBoxButton.YesNo);
            switch (option)
            {
                case MessageBoxResult.Yes:
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                            {

                                EndPlay = false;
                                result.Content = "";
                                main_panel.Children.Clear();
                                Create_ArrayBtn();
                                CreateArr();
                                count = -1;
                                getPos = false;
                                mode = 0;
                            }));
                        break;
                    }
                case MessageBoxResult.No:
                    {
                        EndPlay = true;
                        getPos = false;
                        break;
                    }
            }
        }
        private void Btn_Click(object sender, RoutedEventArgs e)//đi cờ khi offline
        {
            
            {
                string tmp = ((Button)sender).Tag.ToString();
                int len = tmp.Length;
                string i = "";
                string j = "";
                string[] arr_num = tmp.Split(',');
                i += arr_num[0][1];
                if (arr_num[0].Length == 3)
                    i += arr_num[0][2];
                j += arr_num[1][0];
                if (arr_num[1].Length == 3)
                    j += arr_num[1][1];
                int row = Int32.Parse(i);
                int col = Int32.Parse(j);

                if (mode == 1 && !EndPlay)
                {
                    Ellipse el = new Ellipse();
                    el.Height = 30;
                    el.Width = 30;

                    if (arr[row][col] != 0)
                    {
                        return;
                    }

                    if (count % 2 == 0)
                    {
                        el.Fill = System.Windows.Media.Brushes.Black;
                        arr[row][col] = 1;
                    }
                    else
                    {
                        el.Fill = System.Windows.Media.Brushes.SandyBrown;
                        arr[row][col] = 2;
                    }
                    ((Button)sender).Content = el;
                    //Kiểm tra thắng...
                    int check = Check_Winner(row, col);
                    if (check == 1)
                    {
                        PP.IsEnabled = true;
                        pc.IsEnabled = true;
                        pc_Off.IsEnabled = true;
                        PP_Off.IsEnabled = true;
                        string message = "Người chơi 1 chiến thắng!";
                        result.Content = message;
                        result.FontSize = 18;
                        result.FontWeight = FontWeights.Bold;
                        result.Foreground = System.Windows.Media.Brushes.Green;
                        string msg = "Bạn có muốn bắt đầu trò chơi mới không?";
                        MessageBoxResult option = MessageBox.Show(msg, "Gomoku", MessageBoxButton.YesNo);
                        switch (option)
                        {
                            case MessageBoxResult.Yes:
                                {
                                    result.Content = "";
                                    main_panel.Children.Clear();
                                    Create_ArrayBtn();
                                    CreateArr();
                                    count = -1;
                                    EndPlay = false;
                                    mode = 0;
                                    break;
                                }
                            case MessageBoxResult.No:
                                {
                                    EndPlay = true;
                                    break;
                                }
                        }

                    }
                    else
                        if (check == 2)
                        {
                            PP.IsEnabled = true;
                            pc.IsEnabled = true;
                            pc_Off.IsEnabled = true;
                            PP_Off.IsEnabled = true;
                            string message = "Người chơi 2 chiến thắng!";
                            result.Content = message;
                            result.FontSize = 18;
                            result.FontWeight = FontWeights.Bold;
                            result.Foreground = System.Windows.Media.Brushes.Green;
                            string msg = "Bạn có muốn bắt đầu trò chơi mới không?";
                            MessageBoxResult option = MessageBox.Show(msg, "Gomoku", MessageBoxButton.YesNo);
                            switch (option)
                            {
                                case MessageBoxResult.Yes:
                                    {
                                        EndPlay = false;
                                        result.Content = "";
                                        main_panel.Children.Clear();
                                        Create_ArrayBtn();
                                        CreateArr();
                                        count = -1;
                                        mode = 0;
                                        break;
                                    }
                                case MessageBoxResult.No:
                                    {
                                        count = -1;
                                        EndPlay = true;
                                        break;
                                    }
                            }
                        }
                    count++;
                }

                if (mode == 2 && !EndPlay)
                {
                    {
                        if (arr[row][col] == 1 || arr[row][col] == 2)
                            return;
                        Ellipse el = new Ellipse();
                        el.Width = 30;
                        el.Height = 30;
                        el.Fill = System.Windows.Media.Brushes.Black;
                        ((Button)sender).Content = el;
                        arr[row][col] = 1;
                        
                        //Kiểm tra thắng...
                        int check = Check_Winner(row, col);
                        if (check == 1)
                        {
                            PP.IsEnabled = true;
                            pc.IsEnabled = true;
                            pc_Off.IsEnabled = true;
                            PP_Off.IsEnabled = true;
                            string message = "Người chơi chiến thắng!";
                            result.Content = message;
                            result.FontSize = 18;
                            result.FontWeight = FontWeights.Bold;
                            result.Foreground = System.Windows.Media.Brushes.Green;
                            string msg = "Bạn có muốn bắt đầu trò chơi mới không?";
                            MessageBoxResult option = MessageBox.Show(msg, "Gomoku", MessageBoxButton.YesNo);
                            switch (option)
                            {
                                case MessageBoxResult.Yes:
                                    {
                                        getPos = false;
                                        result.Content = "";
                                        count = -1;
                                        main_panel.Children.Clear();
                                        Create_ArrayBtn();
                                        CreateArr();
                                        mode = 0;
                                        EndPlay = false;
                                        break;
                                    }
                                case MessageBoxResult.No:
                                    {
                                        EndPlay = true;
                                        getPos = false;
                                        break;
                                    }
                            }
                        }
                        ComputerPlay();
                    }
                }
            }
        }
        private void ComputerPlay()//máy đi cờ
        {
            int x = -1;
            int y = -1;
            do
            {
                Random rd = new Random();
                x = rd.Next(0, 12);
                y = rd.Next(0, 12);
            } while (arr[x][y] != 0);
            this.Dispatcher.Invoke((Action)(() =>
            {
                foreach (Button press in Btn_Arr)
                {
                    if (press.Name == ("Btn" + x.ToString() + y.ToString()))
                    {
                        Ellipse el2 = new Ellipse();
                        el2.Width = 30;
                        el2.Height = 30;
                        el2.Fill = System.Windows.Media.Brushes.SandyBrown;
                        if (getPos && mode == 2)
                            press.Content = el2;
                        else
                            if (mode == 4)
                                press.Content = el2;
                    }
                }
            }));


            if (flag && getConnected)
            {
                s.Emit("MyStepIs", JObject.FromObject(new { row = x, col = y }));
                arr[x][y] = 1;
            }
            else
                if (!flag)
                    arr[x][y] = 2;
            int check = Check_Winner(x, y);
            if (check == 2)
            {
                PP.IsEnabled = true;
                pc.IsEnabled = true;
                pc_Off.IsEnabled = true;
                PP_Off.IsEnabled = true;
                if (mode == 2)
                {
                    string message = "Máy thắng!";
                    result.Content = message;
                    result.FontSize = 18;
                    result.FontWeight = FontWeights.Bold;
                    result.Foreground = System.Windows.Media.Brushes.Green;
                }
                if (mode == 4)
                    PrintMessage("Computer win the game!", "Server");
                string msg = "Bạn có muốn bắt đầu trò chơi mới không?";
                MessageBoxResult option = MessageBox.Show(msg, "Gomoku", MessageBoxButton.YesNo);
                switch (option)
                {
                    case MessageBoxResult.Yes:
                        {
                            getPos = false;
                            result.Content = "";
                            main_panel.Children.Clear();
                            Create_ArrayBtn();
                            CreateArr();
                            count = -1;
                            mode = 0;
                            break;
                        }
                    case MessageBoxResult.No:
                        {
                            getPos = false;
                            break;
                        }
                }
            }
        }
        private void Btn_Click_Onl(object sender, RoutedEventArgs e)//đi cờ khi online
        {
            if(EndPlay)
                PrintMessage("Not allowed!", "Server");
            if (!EndPlay)
            {
                if (mode == 0)
                    PrintMessage("Choose mode!", "Server");
                else
                    if (turn != player)
                        PrintMessage("This is not your turn!", "Server");
                    else
                    {
                        turn = -1;
                        string tmp = ((Button)sender).Tag.ToString();
                        int len = tmp.Length;
                        string i = "";
                        string j = "";
                        string[] arr_num = tmp.Split(',');
                        i += arr_num[0][1];
                        if (arr_num[0].Length == 3)
                            i += arr_num[0][2];
                        j += arr_num[1][0];
                        if (arr_num[1].Length == 3)
                            j += arr_num[1][1];
                        int row = Int32.Parse(i);
                        int col = Int32.Parse(j);

                        if (getConnected == false)
                            PrintMessage("Not allowed!", "Server");
                        else
                            if (arr[row][col] != 0)
                            {
                                PrintMessage("Invalid step.", "Server");
                                turn = player;
                            }
                            else
                            {
                                if (mode == 3 && getConnected)
                                {
                                    Ellipse el = new Ellipse();
                                    el.Height = 30;
                                    el.Width = 30;

                                    if (check_step == false)
                                    {
                                        PrintMessage("Invalid step.", "Server");
                                        turn = player;
                                    }
                                    else
                                    {
                                        el.Fill = System.Windows.Media.Brushes.SandyBrown;
                                        s.Emit("MyStepIs", JObject.FromObject(new { row = row, col = col }));
                                        check_step = true;
                                        ((Button)sender).Content = el;
                                        arr[row][col] = 1;
                                        //Kiểm tra thắng...
                                        Check_result(row, col);
                                    }
                                }
                            }
                    }
            }
        }       
        private void Getstring(string s)//chuẩn hóa và hiển thị tin nhắn
        {
            string[] getstr = s.Split('<');
            string[] getstr1 = getstr[0].Split('"');
            string[] getstr2 = getstr[1].Split('>');
            string[] getstr3 = getstr2[1].Split('"');
            PrintMessage(getstr1[3] + "\n" + getstr3[0], "Server");
            GetNamePlayer(getstr1[3]);
        }
        private void GetNamePlayer(string s)//lấy tên người chơi online
        {
            if(player == 1)
            {
                int pFrom = s.IndexOf("and ") + "and ".Length;
                int pTo = s.LastIndexOf(" started");
                string result = s.Substring(pFrom, pTo - pFrom);
                namePlayer = result;
            }
            if(player == 0)
            {
                int index = s.LastIndexOf(" and");
                namePlayer = s.Substring(0, index - 0);
            }
        }
        private void Connect()//kết nối
        {
            if (!checkName)
                PrintMessage("Too short name!", "Server");
            else
            {
                flag = true;
                main_panel.Children.Clear();
                Create_ArrayBtn();
                CreateArr();
                Label msg = new Label();
                StackPanel msg_panel = new StackPanel();
                name = txt_name.Text;
                pre_name = txt_name.Text;
                var socket = IO.Socket("ws://gomoku-lajosveres.rhcloud.com:8000");
                s = socket;
                socket.On(Socket.EVENT_CONNECT, () =>
                {
                    MessageBox.Show("Kết nối thành công!");

                });

                socket.On(Socket.EVENT_MESSAGE, (data) =>
                {
                    string m = data.ToString();
                    MessageBox.Show(m);
                });

                socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
                {
                    string m = data.ToString();
                    MessageBox.Show(m);

                });

                socket.On("ChatMessage", (data) =>
                {
                    string m = data.ToString();
                    check_step = true;
                    if (m.Contains("from"))
                    {
                        string[] arr_str = m.Split('"');
                        ms = arr_str[3];
                        name_player = arr_str[7];
                        if (sendMsg != 1)
                        {
                            PrintMessage(ms, name_player);
                        }
                        else
                            if (sendMsg == 1)
                                this.Dispatcher.Invoke((Action)(() =>
                            {
                                {
                                    PrintMessage(txt_msg.Text, txt_name.Text);
                                    sendMsg = 0;
                                }
                            }));
                    }

                    if (m.Contains("Welcome"))
                    {
                        PrintMessage("Welcome!", "Server");
                        PrintMessage("Waiting for other players to connect", "Server");
                    }
                    if (m.Contains("called"))
                    {
                        if (changeName && pre_name != "" && pre_name != name)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                                {
                                    PrintMessage(pre_name + " is now called " + name, "Server");
                                    pre_name = name;
                                }));
                            changeName = false;
                            start = false;
                        }
                        else
                            if (!changeName && !start)
                            {
                                string[] str = m.Split('"');
                                PrintMessage(str[3], "Server");
                                int pFrom = str[3].IndexOf("called ") + "called ".Length;
                                namePlayer = str[3].Substring(pFrom, str[3].Length - pFrom);
                            }
                    }

                    if (m.Contains("first"))
                    {
                        player = 1;
                        turn = player;
                        getConnected = true;
                        start = false;
                        Getstring(m);
                        if (mode == 4)
                            ComputerPlay();
                    }
                    else
                        if (m.Contains("second"))
                        {
                            player = 0;
                            getConnected = true;
                            start = false;
                            Getstring(m);
                        }

                    if (((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() == "Welcome!")
                    {
                        socket.Emit("MyNameIs", name);
                        socket.Emit("ConnectToOtherPlayer");
                    }
                });
                socket.On(Socket.EVENT_ERROR, (data) =>
                {
                    string m = data.ToString();
                    MessageBox.Show(m);
                });
                socket.On("NextStepIs", (data) =>
                {
                    ms = data.ToString();
                    string[] arr_num = ms.Split(':');

                    int x = Int32.Parse(arr_num[1][1].ToString());
                    string getrow = arr_num[2];
                    getrow = getrow.Substring(getrow.IndexOf(" ") + 1);
                    getrow = getrow.Substring(0, getrow.IndexOf(","));

                    string getcol = arr_num[3];
                    getcol = getcol.Substring(getcol.IndexOf(" ") + 1);
                    getcol = getcol.Substring(0, getcol.IndexOf("}"));

                    if (x == 1)
                    {
                        r_rec = Int32.Parse(getrow);
                        c_rec = Int32.Parse(getcol);
                        arr[r_rec][c_rec] = 2;

                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            foreach (Button press in Btn_Arr)
                            {
                                if (press.Name == ("Btn" + r_rec.ToString() + c_rec.ToString()))
                                {
                                    Ellipse el2 = new Ellipse();
                                    el2.Width = 30;
                                    el2.Height = 30;
                                    el2.Fill = System.Windows.Media.Brushes.Black;
                                    press.Content = el2;
                                }
                            }
                        }));
                        int c = Check_Winner(r_rec, c_rec);
                        Check_result(r_rec, c_rec);

                        if (mode == 4 && c == 0)
                        {
                            getPos = true;
                            ComputerPlay();
                        }

                        if (mode == 3)
                            turn = player;
                    }
                });
            }
        }
        private void PrintMessage(string message, string nameplayer)//in thông tin ra khung chat
        {
            if (message == "")
                return;
            this.Dispatcher.Invoke((Action)(() =>
            {
                string time = DateTime.Now.ToString("HH:mm:ss tt");
                StackPanel msg_panel = new StackPanel();
                msg_panel.Orientation = Orientation.Horizontal;
                msg_panel.Orientation = Orientation.Vertical;
                Label msg = new Label();
                Label name = new Label();
                Label space = new Label();
                Label t = new Label();


                string tmp = "--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------";
                space.Content = tmp;
                space.FontSize = 8;

                msg.Content = message;
                msg.FontSize = 14;

                name.Content = nameplayer;
                name.FontSize = 16;
                name.FontWeight = FontWeights.Bold;

                t.Content = time;
                t.FontSize = 10;
                t.FontWeight = FontWeights.Thin;
                t.FontStyle = FontStyles.Italic;

                panel_chat.Children.Add(msg_panel);
                msg_panel.Children.Add(name);
                msg_panel.Children.Add(msg);
                msg_panel.Children.Add(t);
                msg_panel.Children.Add(space);
                txt_msg.Clear();
                
            }));
        }
        private void Button_Click(object sender, RoutedEventArgs e)//gửi tin nhắn
        {
            if (mode > 2 && getConnected && txt_msg.Text != "")
            {
                s.Emit("ChatMessage", txt_msg.Text);
                sendMsg = 1;
            }
            else
                 PrintMessage(txt_msg.Text, txt_name.Text);
        }
        private void Btn_change_Click(object sender, RoutedEventArgs e)//đổi tên
        {
            if (txt_name.Text == "")
            {
                PrintMessage("Too short name!", "Server");
                checkName = false;
            }
            else
            {
                checkName = true;
                name = txt_name.Text;
                if (flag)
                {
                    s.Emit("MyNameIs", name);
                    changeName = true;
                }
                else
                    PrintMessage("Name change: " + name, "Server");
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)//chọn chế độ người vs người offline
        {
            PP.IsEnabled = false;
            pc.IsEnabled = false;
            pc_Off.IsEnabled = false;
            main_panel.Children.Clear();
            Create_ArrayBtn();
            CreateArr();
            mode = 1;
            EndPlay = false;
            result.Content = "";
            count = 0;
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)//chọn chế độ người vs máy offline
        {
            PP.IsEnabled = false;
            pc.IsEnabled = false;
            PP_Off.IsEnabled = false;
            main_panel.Children.Clear();
            Create_ArrayBtn();
            CreateArr();
            mode = 2;
            EndPlay = false;
            getPos = true;
            result.Content = "";
        }
        private void PP_Click(object sender, RoutedEventArgs e)//chọn chế độ người vs người online
        {
            if (!checkName || txt_name.Text == "")
                PrintMessage("Too short name!", "Server");
            else
            {
                PP_Off.IsEnabled = false;
                pc_Off.IsEnabled = false;
                pc.IsEnabled = false;
                main_panel.Children.Clear();
                Create_ArrayBtn();
                CreateArr();
                Connect();
                mode = 3;
                EndPlay = false;
                result.Content = "";
                start = true;
                turn = -1;
            }
        }
        private void pc_Click(object sender, RoutedEventArgs e)//chọn chế độ người vs máy online
        {
            if (!checkName || txt_name.Text == "")
                PrintMessage("Too short name!", "Server");
            else
            {
                PP.IsEnabled = false;
                pc_Off.IsEnabled = false;
                PP_Off.IsEnabled = false;
                main_panel.Children.Clear();
                Create_ArrayBtn();
                CreateArr();
                Connect();
                mode = 4;
                EndPlay = false;
                result.Content = "";
            }
        }
        private void Btn_Cnn_Click(object sender, RoutedEventArgs e)//thoát trò chơi
        {
            Application.Current.Shutdown();
        }
    }
}