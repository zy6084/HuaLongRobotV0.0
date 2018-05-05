using System;
using System.Collections;
using System.Windows.Forms;

namespace TcpServcerTest
    {
    public partial class FrmModeSelectLogin : Form
        {
        public FrmModeSelectLogin()
            {
            InitializeComponent();
            }

        private void btnLogin_Click(object sender, EventArgs e)
            {
            if (txtLogin.Text=="3121858")
                {
                FrmModeSelect ms = new FrmModeSelect();
                ms.Show();
                this.Dispose();
                }
            }

        private void txtLogin_KeyDown(object sender, KeyEventArgs e)
            {
            if (e.KeyCode == Keys.Enter)
                {
                this.btnLogin.Focus();
                }

            }
        }
    }
