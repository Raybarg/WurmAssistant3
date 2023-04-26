﻿using AldursLab.WurmApi;
using AldursLab.WurmAssistant3.Areas.Logging;
using AldursLab.WurmAssistant3.Areas.Sermoner.Sermon;
using AldursLab.WurmAssistant3.Utils.WinForms;
using BrightIdeasSoftware;
using JetBrains.Annotations;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AldursLab.WurmAssistant3.Areas.Sermoner
{
    public partial class SermonerForm : ExtendedForm
    {
        readonly ILogger logger;
        readonly IWurmApi wurmApi;
        readonly SermonerSettings sermonerSettings;

        PreacherList preacherList;

        private bool SessionRunning = false;
        private string CharacterString;

        public SermonerForm(
            [NotNull] ILogger logger,
            [NotNull] IWurmApi wurmApi,
            [NotNull] SermonerSettings sermonerSettings
            )
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (wurmApi == null) throw new ArgumentNullException("wurmApi");
            this.logger = logger;
            this.wurmApi = wurmApi;
            this.sermonerSettings = sermonerSettings;
            InitializeComponent();

            var allChars = this.wurmApi.Characters.All.Select(character => character).ToArray();
            liveMonCharacterCbox.Items.AddRange(
                allChars.Select(character => character.Name.Capitalized).Cast<object>().ToArray());

            lblCooldown.Text = String.Empty;
            lblCooldown.Visible = false;
        }

        void ShowError(Exception exception)
        {
            MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void RefreshPreacherList()
        {
            if (sermonerSettings.OmitOldPreacherTime > 0)
                preacherList.ClearOldSermons(sermonerSettings.OmitOldPreacherTime);
            olvPreachers.SetObjects(preacherList);
            olvPreachers.Sort(olvColumn2);

            if (preacherList.IsOnCooldown)
            {
                lblCooldown.Text = $"Sermoning appears to be on cooldown for {preacherList.CoolDownLeft} more minutes.";
                lblCooldown.ForeColor = Color.Firebrick;
            }
            else
            {
                lblCooldown.Text = "Can sermon now!";
                lblCooldown.ForeColor = Color.DarkGreen;
            }
        }

        #region "Events"

        private void startLiveSessionBtn_Click(object sender, EventArgs e)
        {
            if (SessionRunning)
            {
                StopSession();
                liveMonCharacterCbox.Enabled = true;
                startLiveSessionBtn.Text = "Start session";
                SessionRunning = false;
            }
            else
            {
                StartSession();
                liveMonCharacterCbox.Enabled = false;
                startLiveSessionBtn.Text = "Stop session";
                SessionRunning = true;
            }
        }

        private void StartSession()
        {
            try
            {
                CharacterString = liveMonCharacterCbox.Text;
                if (string.IsNullOrWhiteSpace(CharacterString))
                {
                    MessageBox.Show("Please choose game character");
                    return;
                }
                var gameCharacter = wurmApi.Characters.Get(CharacterString);

                if (preacherList == null)
                {
                    preacherList = new PreacherList(gameCharacter, logger);
                    preacherList.AsyncInitCompleted += this.AsyncPreacherListInitCompleted;
                }
                else
                {
                    preacherList.Clear();
                }

                preacherList.PerformAsyncInit();
                wurmApi.LogsMonitor.Subscribe(CharacterString, LogType.AllLogs, OnNewLogEvents);
            }
            catch (Exception exception)
            {
                ShowError(exception);
                logger.Error(exception, "Sermoner live session start failed");
            }
        }

        private void StopSession()
        {
            wurmApi.LogsMonitor.Unsubscribe(CharacterString, OnNewLogEvents);
        }


        void OnNewLogEvents(object sender, LogsMonitorEventArgs e)
        {
            if (e.LogType == LogType.Event)
            {
                foreach (var entry in e.WurmLogEntries)
                {
                    preacherList.HandleLogEntry(entry);
                }
            }
        }

        private void olvPreachers_FormatRow(object sender, FormatRowEventArgs e)
        {
            Preacher p = (Preacher)e.Model;
            if (p.CooldownLeft() > 0)
            {
                e.Item.BackColor = Color.LightPink;
            }
            else
            {
                e.Item.BackColor = Color.LightGreen;
            }
        }

        private void AsyncPreacherListInitCompleted(object sender, EventArgs e)
        {
            RefreshPreacherList();
            timer.Enabled = true;
            lblCooldown.Visible = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            RefreshPreacherList();
        }

        private void SermonerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Hide();
                e.Cancel = true;
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            SermonerSettingsForm settings = new SermonerSettingsForm(this, sermonerSettings);
            settings.ShowDialogCenteredOnForm(this);
        }

        #endregion

    }
}
