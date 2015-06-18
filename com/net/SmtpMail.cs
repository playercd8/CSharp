using System;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;

//連線到SMTP Server的相關設定, 定義在 web.config 或 app.config 裡
/*
<system.net>
<mailSettings>
    <smtp deliveryMethod="Network" from="寄件者的email">
        <network defaultCredentials="false" host="SMTP主機的IP" port="25" userName="帳號" password="密碼" />
    </smtp>
</mailSettings>
</system.net>
*/

namespace com.net
{
    /// <summary>
    /// SmtpMail 的摘要描述
    /// </summary>
    public class SmtpMail : IDisposable
    {
        public SmtpMail()
        {
        }

        private SmtpClient smtpClient = new SmtpClient();
        private MailMessage message = new MailMessage();
        private string errorMessage = "";

        #region 寄件者

        public void From(string address)
        {
            message.From = new MailAddress(address);
        }

        public void From(string address, string displayName)
        {
            message.From = new MailAddress(address, displayName);
        }

        public void Sender(string address)
        {
            message.Sender = new MailAddress(address);
        }

        public void Sender(string address, string displayName)
        {
            message.Sender = new MailAddress(address, displayName);
        }

        #endregion 寄件者

        #region 收件者

        public void To(string address)
        {
            message.To.Add(address);
        }

        public void To(string address, string displayName)
        {
            message.To.Add(new MailAddress(address, displayName));
        }

        #endregion 收件者

        #region 副本 (CC)

        public void CC(string address)
        {
            message.CC.Add(address);
        }

        public void CC(string address, string displayName)
        {
            message.CC.Add(new MailAddress(address, displayName));
        }

        #endregion 副本 (CC)

        #region 密件副本 (BCC)

        public void Bcc(string address)
        {
            message.Bcc.Add(address);
        }

        public void Bcc(string address, string displayName)
        {
            message.Bcc.Add(new MailAddress(address, displayName));
        }

        #endregion 密件副本 (BCC)

        #region 郵件訊息回覆時的地址

        public void ReplyTo(string address)
        {
            message.ReplyToList.Add(address);
        }

        public void ReplyTo(string address, string displayName)
        {
            message.ReplyToList.Add(new MailAddress(address, displayName));
        }

        #endregion 郵件訊息回覆時的地址

        /// <summary>
        /// 主旨
        /// </summary>
        public string Subject
        {
            get { return message.Subject; }
            set { message.Subject = value; }
        }

        /// <summary>
        /// 訊息主體
        /// </summary>
        public string Body
        {
            get { return message.Body; }
            set { message.Body = value; }
        }

        /// <summary>
        /// 郵件訊息主體是否採用 Html 格式
        /// </summary>
        public bool IsBodyHtml
        {
            get { return message.IsBodyHtml; }
            set { message.IsBodyHtml = value; }
        }

        /// <summary>
        /// 優先權
        /// </summary>
        public MailPriority Priority
        {
            get { return message.Priority; }
            set { message.Priority = value; }
        }

        #region 檔案附件

        public void Attachments(string file)
        {
            Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
            // Add time stamp information for the file.
            ContentDisposition disposition = data.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(file);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
            // Add the file attachment to this e-mail message.
            message.Attachments.Add(data);
        }

        public void Attachments(System.IO.Stream contentStream, ContentType contentType, string fileName)
        {
            // Attach the log file stream to the e-mail message.
            Attachment data = new Attachment(contentStream, contentType);
            ContentDisposition disposition = data.ContentDisposition;
            disposition.FileName = fileName;
            // Add the attachment to the message.
            message.Attachments.Add(data);
        }

        #endregion 檔案附件

        public bool Send()
        {
            try
            {
                smtpClient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public string GetLastErrorMessage()
        {
            return errorMessage;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.message.Dispose();
                this.smtpClient.Dispose();
            }
        }
    }
}