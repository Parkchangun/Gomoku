using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class SinglePlayForm : Form
    {
        //15 x 15 바둑판 형태로 구성
        private const int rectSize = 33; //오목판의 셀 크기
        private const int edgeCount = 15; //오목판의 선 개수

        private enum Horse {none = 0, BLACK, WHITE};
        //15 x 15 크기의 2차원 배열 board
        private Horse[,] board = new Horse[edgeCount, edgeCount];
        //플레이어 지정
        private Horse nowPlayer = Horse.BLACK;

        private bool playing = false;

        public SinglePlayForm()
        {
            InitializeComponent();
        }

        //승리 판정 함수
        private bool Judge()
        {
            //가로 5칸
            for(int i = 0; i < edgeCount - 4; i++)
            {
                for(int j = 0; j < edgeCount; j++)
                {
                    if(board[i, j] == nowPlayer && board[i + 1, j] == nowPlayer && board[i + 2, j] == nowPlayer
                        &&board[i + 3, j] == nowPlayer && board[i + 4, j] == nowPlayer)
                    {
                        return true;
                    }
                }
            }
            //세로 5칸
            for (int i = 0; i < edgeCount - 4; i++)
            {
                for (int j = 4; j < edgeCount; j++)
                {
                    if (board[i, j] == nowPlayer && board[i, j - 1] == nowPlayer && board[i, j - 2] == nowPlayer
                        && board[i, j - 3] == nowPlayer && board[i, j - 4] == nowPlayer)
                    {
                        return true;
                    }
                }
            }
            //Y = X 대각선
            for (int i = 0; i < edgeCount - 4; i++)
            {
                for (int j = 0; j < edgeCount; j++)
                {
                    if (board[i, j] == nowPlayer && board[i + 1, j + 1] == nowPlayer && board[i + 2, j + 2] == nowPlayer
                        && board[i + 3, j + 3] == nowPlayer && board[i + 4, j + 4] == nowPlayer)
                    {
                        return true;
                    }
                }
            }
            //Y = -X 대각선
            for (int i = 4; i < edgeCount - 4; i++)
            {
                for (int j = 0; j < edgeCount; j++)
                {
                    if (board[i, j] == nowPlayer && board[i - 1, j + 1] == nowPlayer && board[i - 2, j + 2] == nowPlayer
                        && board[i - 3, j + 3] == nowPlayer && board[i - 4, j + 4] == nowPlayer)
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
            for(int i = 0; i < edgeCount; i++)
            {
                for(int j = 0; j < edgeCount; j++)
                {
                    board[i, j] = Horse.none;
                }
            }

        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (!playing)
            {
                BoardRefresh();
                playing = true;
                playButton.Text = "재시작";
                status.Text = nowPlayer.ToString() + "플레이어의 차례입니다.";
            }
            else
            {
                Refresh();
                status.Text = "게임이 재시작되었습니다.";
            }
        }

        private void boardPicture_MouseDown(object sender, MouseEventArgs e)
        {
            if (!playing)
            {
                MessageBox.Show("게임을 실행해주세요");
                return;
            }

            Graphics g = this.boardPicture.CreateGraphics();
            int x = e.X / rectSize;
            int y = e.Y / rectSize;

            if(x < 0 || y < 0 || x >= edgeCount || y >= edgeCount)
            {
                MessageBox.Show("테두리를 벗어날 수 없습니다.");
                return;
            }

            if (board[x, y] != Horse.none)
            {
                MessageBox.Show("놓을 수 없는 위치입니다");
                return;
            }

            MessageBox.Show(x + ", " + y);
            board[x, y] = nowPlayer;

            if(nowPlayer == Horse.BLACK)
            {
                SolidBrush brush = new SolidBrush(Color.Black);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }
            else
            {
                SolidBrush brush = new SolidBrush(Color.White);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }

            //현재 플레이어가 돌을 놓은 후
            if (Judge())
            {
                status.Text = nowPlayer.ToString() + "플레이어가 승리했습니다.";
                playing = false;
                playButton.Text = "게임 시작";
            }
            else
            {
                nowPlayer = (nowPlayer == Horse.BLACK) ? Horse.WHITE : Horse.BLACK;
                status.Text = nowPlayer.ToString() + "플레이어의 차례입니다";
            }
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
    }
}
