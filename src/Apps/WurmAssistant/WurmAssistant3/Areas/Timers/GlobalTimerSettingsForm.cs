﻿using System;
using System.Windows.Forms;
using AldursLab.WurmAssistant3.Utils.WinForms;

namespace AldursLab.WurmAssistant3.Areas.Timers
{
    public partial class GlobalTimerSettingsForm : ExtendedForm
    {
        private readonly TimersForm timersView;

        public GlobalTimerSettingsForm(TimersForm timersView)
        {
            this.timersView = timersView;
            InitializeComponent();

            checkBoxWidgetView.Checked = timersView.WidgetModeEnabled;
            textBoxWidgetSample.BackColor = timersView.WidgetBgColor;
            textBoxWidgetSample.ForeColor = timersView.WidgetForeColor;
            checkBoxShowEndDate.Checked = timersView.ShowEndDate;
            checkBoxShowEndDateInstead.Checked = timersView.ShowEndDateInsteadOfTimeRemaining;
            UpdateControls();
            SetBarColorMode(this.timersView.BarColorMode);
        }

        void UpdateControls()
        {
            if (!checkBoxShowEndDate.Checked)
            {
                checkBoxShowEndDateInstead.Checked = false;
            }
            checkBoxShowEndDateInstead.Enabled = checkBoxShowEndDate.Checked;
        }

        private void checkBoxWidgetView_CheckedChanged(object sender, EventArgs e)
        {
            timersView.WidgetModeEnabled = checkBoxWidgetView.Checked;
        }

        private void buttonChangeWidgetBgColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = timersView.WidgetBgColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                timersView.WidgetBgColor = colorDialog1.Color;
                textBoxWidgetSample.BackColor = colorDialog1.Color;
            }
        }

        private void textBoxWidgetSample_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonSetWidgetFontColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = timersView.WidgetForeColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                timersView.WidgetForeColor = colorDialog1.Color;
                textBoxWidgetSample.ForeColor = colorDialog1.Color;
            }
        }

        private void buttonResetWidgetDefaultColor_Click(object sender, EventArgs e)
        {
            textBoxWidgetSample.BackColor = timersView.WidgetBgColor = DefaultBackColor;
            textBoxWidgetSample.ForeColor = timersView.WidgetForeColor = DefaultForeColor;
        }

        private void checkBoxShowEndDate_CheckedChanged(object sender, EventArgs e)
        {
            timersView.ShowEndDate = checkBoxShowEndDate.Checked;
            UpdateControls();
        }

        private void checkBoxShowEndDateInstead_CheckedChanged(object sender, EventArgs e)
        {
            timersView.ShowEndDateInsteadOfTimeRemaining = checkBoxShowEndDateInstead.Checked;
            UpdateControls();
        }

        private void SetBarColorMode(int barColorMode)
        {
            switch(barColorMode)
            {
                case 0:
                    rbnColorSimple.Checked = true;
                    break;
                case 1:
                    rbnColorGreenReady.Checked = true;
                    break;
                case 2:
                    rbnColorRedReady.Checked = true;
                    break;
            }
        }

        private void rbnColor_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton == rbnColorSimple)
            {
                this.timersView.BarColorMode = 0;
            } else if (radioButton == rbnColorGreenReady)
            {
                this.timersView.BarColorMode = 1;
            } else if (radioButton == rbnColorRedReady)
            {
                this.timersView.BarColorMode = 2;
            }
        }
    }
}
