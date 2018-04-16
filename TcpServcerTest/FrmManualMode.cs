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
            OnOrderChanged(new OrderTypeEventArgs("ZQ1B"));
            }

        private void btnExit_Click(object sender, EventArgs e)
            {
            this.Close();
            }

        private void btnReturn1_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("Return1"));
            }

        private void btnZq3b_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("ZQ3B"));
            }

        private void btnJQ2_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("JQ2"));
            }

        private void btnReturn3_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("Return3"));
            }

        private void btnZq2b_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("ZQ2B"));
            }

        private void btnJQ1_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("JQ1"));
            }

        private void btnReturn2_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("Return2"));
            }

        private void btnTC_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("TopCaliperClamp"));
            }

        private void btnLC_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("LowCaliperClamp"));
            }

        private void btnTO_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("TopCaliperOpen"));
            }

        private void btnLO_Click(object sender, EventArgs e)
            {
            OnOrderChanged(new OrderTypeEventArgs("LowCaliperOpen"));
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
