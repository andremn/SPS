﻿using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SPS.Web.Models;
using SendGrid;
using System.Net;
using System.Configuration;
using System.Web;
using System.Net.Mail;
using System.Net.Mime;
using System;
using SPS.Web.Common;

namespace SPS.Web
{
    // Configure o gerenciador de usuários do aplicativo usado nesse aplicativo. O UserManager está definido no ASP.NET Identity e é usado pelo aplicativo.

    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configurar a lógica de validação para nomes de usuário
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configurar a lógica de validação para senhas
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            // Registre dois provedores de autenticação de fator. Este aplicativo usa Telefone e Emails como um passo para receber um código para verificar o usuário
            // Você pode gravar seu próprio provedor e se conectar aqui.
            manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is: {0}"
            });
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Código de segurança",
                BodyFormat = "Your security code is: {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.Run(() =>
                {
                    var msg = new MailMessage()
                    {
                        From = new MailAddress("sps@engineer.com", "Smart Parking System"),
                        Subject = message.Subject
                    };

                    msg.To.Add(new MailAddress(message.Destination));
                    msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.Body, null, MediaTypeNames.Text.Plain));
                    msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.Body, null, MediaTypeNames.Text.Html));

                    var smtpClient = new SmtpClient(ApplicationConfig.MailSMTPServerAddress, Convert.ToInt32(587))
                    {
                        Credentials = new NetworkCredential(ApplicationConfig.SPSMail, ApplicationConfig.SPSMailPassword),
                        EnableSsl = true
                    };

                    smtpClient.Send(msg);
                });
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.Run(() =>
            {
                string toPhoneNumber = "DestinationPhoneNumber";
                string login = "YoureIPIPIUsername";
                string password = "YourPassword";
                string compression = "Compression Option goes here - find out more";
                string body = "Your Message";


                System.Web.Mail.MailMessage mail = new System.Web.Mail.MailMessage();
                mail.To = toPhoneNumber + "@sms.ipipi.com";
                mail.From = login + "@ipipi.com";
                mail.Subject = compression;
                mail.Body = body;
                mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
                mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", login);
                mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", password);


                System.Web.Mail.SmtpMail.SmtpServer = "ipipi.com";
                System.Web.Mail.SmtpMail.Send(mail);
            });
        }
    }
}