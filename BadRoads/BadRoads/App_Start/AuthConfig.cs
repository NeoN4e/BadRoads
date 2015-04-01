using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using BadRoads.Models;
using BadRoads.VKOauth;

namespace BadRoads
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            OAuthWebSecurity.RegisterFacebookClient(
                appId: "1008370482524856",
                appSecret: "523dd07b2685d973af50bd79440764ea");

            OAuthWebSecurity.RegisterClient(
       client: new VKontakteAuthenticationClient(
              "4852475", "ulGQqEeM9UNb7pCqORzp"),
       displayName: "ВКонтакте", // надпись на кнопке
       extraData: null);
            OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
