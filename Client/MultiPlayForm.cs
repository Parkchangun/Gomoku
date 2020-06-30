using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    public partial class MultiPlayForm : Form
    {
        private Thread thread; // 통신을 위한 쓰레드
        private TcpClient tcpClient; // TCP 클라이언트
        private NetworkStream stream;

        //15 x 15 바둑판 형태로 구성
        private const int rectSize = 33; //오목판의 셀 크기
        private const int edgeCount = 15; //오목판의 선 개수

        private enum Horse { none = 0, BLACK, WHITE };
        //15 x 15 크기의 2차원 배열 board
        private Horse[,] board;
        private Horse nowPlayer;
        private bool nowTurn;

        private bool entered;
        private bool playing;
        private bool threading;

        public MultiPlayForm()
        {
            InitializeComponent();
            this.playButton.Enabled = false;
            playing = false;
            entered = false;
            threading = false;
            board = new Horse[edgeCount, edgeCount];
            nowTurn = false;
        }

        //승리 판정 함수
        private bool Judge(Horse player)
        {
            //가로 5칸
            for (int i = 0; i < edgeCount - 4; i++)
            {
                for (int j = 0; j < edgeCount; j++)
                {
                    if (board[i, j] == player && board[i + 1, j] == player && board[i + 2, j] == player
                        && board[i + 3, j] == player && board[i + 4, j] == player)
                    {
                        return true;
                    }
                }
            }
            //세로 5칸
            for (int i = 0; i < edgeCount; i++)
            {
                for (int j = 0; j < edgeCount - 4; j++)
                {
                    if (board[i, j] == player && board[i, j + 1] == player && board[i, j + 2] == player
                        && board[i, j + 3] == player && board[i, j + 4] == player)
                    {
                        return true;
                    }
                }
            }
            //Y = X 대각선
            for (int i = 0; i < edgeCount - 4; i++)
            {
                for (int j = 0; j < edgeCount - 4; j++)
                {
                    if (board[i, j] == player && board[i + 1, j + 1] == player && board[i + 2, j + 2] == player
                        && board[i + 3, j + 3] == player && board[i + 4, j + 4] == player)
                    {
                        return true;
                    }
                }
            }
            //Y = -X 대각선
            for (int i = 4; i < edgeCount; i++)
            {
                for (int j = 0; j < edgeCount - 4; j++)
                {
                    if (board[i, j] == player && board[i - 1, j + 1] == player && board[i - 2, j + 2] == player
                        && board[i - 3, j + 3] == player && board[i - 4, j + 4] == player)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        //초기화
        private void BoardRefresh()
        {
            this.boardPicture.Refresh();
            for (int i = 0; i < edgeCount; i++)
            {
                for (int j = 0; j < edgeCount; j++)
                {
                    board[i, j] = Horse.none;
                }
            }
            playButton.Enabled = false;
        }
        /* 서버로부터 메시지를 전달 받는 처리를 할 함수 */
        private void ReadMessage()
        {
            while (true)
            {
                byte[] buf = new byte[1024];
                int bufBytes = stream.Read(buf, 0, buf.Length);
                string message = Encoding.ASCII.GetString(buf, 0, bufBytes);

                /* 접속 성공(메시지: [Enter]) */
                if (message.Contains("[Enter]"))
                {
                    this.status.Text = "[" + this.roomTextBox.Text + "]번 방에 접속했습니다";
                    //게임 시작 처리
                    this.roomTextBox.Enabled = false;
                    this.enterButton.Enabled = false;
                    entered = true;
                }
                /* 방이 가득 찬 경우(메시지: [Full]) */
                if (message.Contains("[Full]"))
                {
                    this.status.Text = "이미 가득 찬 방입니다";
                    closeNetwork();
                }
                /* 게임 시작(메시지: [Play]{Horse}) */
                if (message.Contains("[Play]"))
                {
                    BoardRefresh();
                    string horse = message.Split(']')[1];

                    if (horse.Contains("Black"))
                    {
                        this.status.Text = "당신의 차례입니다";
                        nowTurn = true;
                        nowPlayer = Horse.BLACK;
                    }
                    else
                    {
                        this.status.Text = "상대방의 차례입니다";
                        nowTurn = false;
                        nowPlayer = Horse.WHITE;
                    }
                    playing = true;
                }
                /* 상대방이 나간 경우(메시지: [Exit]) */
                if (message.Contains("[Exit]"))
                {
                    this.status.Text = "상대방이 나갔습니다";
                    BoardRefresh();
                }
                /* 상대방이 돌을 둔 경우(메시지: [Put]{X,Y} */
                if (message.Contains("[Put]"))
                {
                    string position = message.Split(']')[1];
                    int x = Convert.ToInt32(position.Split(',')[0]);
                    int y = Convert.ToInt32(position.Split(',')[1]);

                    Horse enemyPlayer = Horse.none;

                    if(nowPlayer == Horse.BLACK)
                    {
                        enemyPlayer = Horse.WHITE;
                    }
                    else
                    {
                        enemyPlayer = Horse.BLACK;
                    }

                    if (board[x, y] != Horse.none) continue;
                    board[x, y] = enemyPlayer;

                    Graphics g = this.boardPicture.CreateGraphics();
                    if(enemyPlayer == Horse.BLACK)
                    {
                        SolidBrush brush = new SolidBrush(Color.Black);
                        g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
                    }
                    else
                    {
                        SolidBrush brush = new SolidBrush(Color.White);
                        g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
                    }

                    if (Judge(enemyPlayer))
                    {
                        status.Text = "패배";
                        playing = false;
                        playButton.Text = "재시작";
                        playButton.Enabled = true;
                    }
                    else
                    {
                        status.Text = "당신의 차례입니다";
                    }
                    nowTurn = true;
                }
            }
        }

        private void enterButton_Click(object sender, EventArgs e)
        {
            tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1", 9876);
            stream = tcpClient.GetStream();

            thread = new Thread(new ThreadStart(ReadMessage));
            thread.Start();
            threading = true;

            /* 방 접속 진행하기 */
            string message = "[Enter]";
            byte[] buf = Encoding.ASCII.GetBytes(message + this.roomTextBox.Text);
            stream.Write(buf, 0, buf.Length);
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (!playing)
            {
                BoardRefresh();
                playing = true;
                string message = "[Play]";
                byte[] buf = Encoding.ASCII.GetBytes(message + this.roomTextBox.Text);
                stream.Write(buf, 0, buf.Length);
                this.status.Text = "상대 플레이어의 준비를 기다립니다";
                this.playButton.Enabled = false;
            }
        }

        private void boardPicture_MouseDown(object sender, MouseEventArgs e)
        {     
            if (!playing)
            {
                MessageBox.Show("게임을 실행해주세요");
                return;
            }

            if (!nowTurn)
            {
                return;
            }

            Graphics g = this.boardPicture.CreateGraphics();
            int x = e.X / rectSize;
            int y = e.Y / rectSize;

            if (x < 0 || y < 0 || x >= edgeCount || y >= edgeCount)
            {
                MessageBox.Show("테두리를 벗어날 수 없습니다.");
                return;
            }

            if (board[x, y] != Horse.none)
            {
                MessageBox.Show("놓을 수 없는 위치입니다");
                return;
            }

            board[x, y] = nowPlayer;

            if (nowPlayer == Horse.BLACK)
            {
                SolidBrush brush = new SolidBrush(Color.Black);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }
            else
            {
                SolidBrush brush = new SolidBrush(Color.White);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }
            /* 놓은 바둑돌의 위치 보내기 */
            string message = "[Put]" + roomTextBox.Text + "," + x + "," + y;
            byte[] buf = Encoding.ASCII.GetBytes(message);
            stream.Write(buf, 0, buf.Length);

            /* 판정 처리 */
            if (Judge(nowPlayer))
            {
                status.Text = "승리";
                playing = false;
                playButton.Text = "재시작";
                playButton.Enabled = true;
                return;
            }
            else
            {
                status.Text = "상대방 차례";
            }
            /* 상대방 차례로 설정 */
            nowTurn = false;
        }

        private void boardPicture_Paint(object sender, PaintEventArgs e)
        {
            Graphics gp = e.Graphics;
            Color lineColor = Color.Black; //오목판의 선 색깔
            Pen p = new Pen(lineColor, 2);

            gp.DrawLine(p, rectSize / 2, rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2);
            gp.DrawLine(p, rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize / 2);
            gp.DrawLine(p, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize / 2, rectSize / 2);
            gp.DrawLine(p, rectSize * edgeCount - rectSize / 2, rectSize / 2, rectSize / 2, rectSize / 2);

            p = new Pen(lineColor, 1);
            //대각선 방향으로 이동하며 십자가 모양의 선 그리기
            for (int i = rectSize + rectSize / 2; i < rectSize * edgeCount - rectSize / 2; i += rectSize)
            {
                gp.DrawLine(p, rectSize / 2, i, rectSize * edgeCount - rectSize / 2, i);
                gp.DrawLine(p, i, rectSize / 2, i, rectSize * edgeCount - rectSize / 2);
            }
        }

        private void MultiPlayForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeNetwork();
        }

        void closeNetwork()
        {
            if (threading && thread.IsAlive) thread.Abort();
            if (entered)
            {
                tcpClient.Close();
            }
        }
    }
}
