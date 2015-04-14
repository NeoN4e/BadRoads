using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using BadRoads.Models;

namespace BadRoads.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Ensure ASP.NET Simple Membership is initialized only once per app start
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                Database.SetInitializer<UsersContext>(null);

                try
                {
                    BadroadsDataContext DB = new BadroadsDataContext();
                    using (var context = new UsersContext())
                    {
                        if (!context.Database.Exists())
                        {
                            // Create the SimpleMembership database without Entity Framework migration schema
                            //((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                    
                            DB.Database.CreateIfNotExists();
                        }
                    }

                    DB.Database.Initialize(true);
                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
                   
                    //Начавльное наполнение БД
                    if (!System.Web.Security.Roles.RoleExists("Administator"))
                    {
                        //Создание пользователя Admin
                        WebMatrix.WebData.WebSecurity.CreateUserAndAccount("Admin", "Admin", new { Email = "Admin@BadRoads.dp.ua" }); // Add Email to defoult Accaunt Table (UserProfile)

                        //Создание ролей
                        System.Web.Security.Roles.CreateRole("Administator");
                        System.Web.Security.Roles.CreateRole("Moderator");
                        System.Web.Security.Roles.CreateRole("User");

                        //Назначение ролей админу
                        System.Web.Security.Roles.AddUserToRoles("Admin", new[] { "User", "Moderator", "Administator" });
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }
            }
        }
    }
}
