using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TcpServcerTest
    {
    public partial class FrmModeSelect : Form
        {
        public FrmModeSelect()
            {
            InitializeComponent();
            }

        private void btnTest_Click(object sender, EventArgs e)
            {
            Properties.Settings.Default.类别 = "0";
            Properties.Settings.Default.Save();
            }

        private void btnFormal_Click(object sender, EventArgs e)
            {
            Properties.Settings.Default.类别 = "1";
            Properties.Settings.Default.Save();
            //Configuration config= ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //ConfigurationSectionGroup configSG = config.SectionGroups["userSettings"];
            //ConfigurationSection configS = configSG.Sections["TcpServcerTest.Properties.Settings"];
            //ClientSettingsSection css = configS as ClientSettingsSection;
            //if (css!=null)
            //    {
            //    SettingElement element1 = css.Settings.Get("类别");
            //    if (element1!=null)
            //        {
            //        element1.Value.ValueXml.InnerXml = "1";
            //        }              
            //    }
            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("userSettings");
            //textBox1.Text = css.Settings.Get("类别").Value.ValueXml.InnerXml;
            }

        private void btnExit_Click(object sender, EventArgs e)
            {
            this.Dispose();
            }
        }
    }
