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
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
        }

        //혼자하기 버튼 클릭 시
        private void singlePlayButton_Click(object sender, EventArgs e)
        {
            Hide(); //현재 창 숨김
            //SinglePlayForm 객체 생성
            SinglePlayForm singlePlayForm = new SinglePlayForm();
            //SinglePlayForm이 닫혀있을 때 childForm_Closed 수행
            singlePlayForm.FormClosed += new FormClosedEventHandler(childForm_Closed);
            //새로운 창 호출
            singlePlayForm.Show();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        void childForm_Closed(object sender, FormClosedEventArgs eventArgs)
        {
            Show();
        }
    }
}
