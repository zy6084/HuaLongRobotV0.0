using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TcpServcerTest
    {
    public partial class FrmManualMode : Form
        {
        public event EventHandler<OrderTypeEventArgs> ManualOperationOrders;

        public FrmManualMode()
            {
            InitializeComponent();            
            }

        protected virtual void OnOrderChanged(OrderTypeEventArgs e)
            {
            ManualOperationOrders?.Invoke(this, e);
            }

        private void btnzq1b_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("zq1b"));
            }

        private void btnExit_Click(object sender, EventArgs e)
            {
            this.Close();
            }
        }
    public class OrderTypeEventArgs : EventArgs
        {
        public string orderName;
        public OrderTypeEventArgs(string order)
            {
            orderName = order;
            }
        }
    }
