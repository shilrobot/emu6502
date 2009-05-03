using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Emu6502
{
    public partial class AddressEntryDlg : Form
    {
        public int Address;

        public AddressEntryDlg()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool addrValid = false;
            try
            {
                Address = Convert.ToUInt16(addressInput.Text, 16);
                addrValid = true;
            }
            catch
            {
            }

            if (addrValid)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Please enter a valid address from $0000 to $FFFF");
            }
        }


    }
}
