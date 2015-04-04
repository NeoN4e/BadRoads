using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace BadRoads.Models
{
    /// <summary>
    /// Вспомогательный клас для инициализации БД + НАполнение первоначальными данными
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    class DbInitializer : IDatabaseInitializer<BadroadsDataContext> //DropCreateDatabaseAlways<BadroadsDataContext>//CreateDatabaseIfNotExists<BadroadsDataContext> IDatabaseInitializer<BadroadsDataContext> //
    {
        public void InitializeDatabase(BadroadsDataContext context)
        {
            if (context.Database.Exists())
            {
                context.Database.Delete();
            }
            context.Database.Create();

            //Установим сортировку
            string DbName = context.Database.Connection.Database;
            context.Defects.Add(new Defect() { Name="яма"});
            context.SaveChanges();

            //context.Database.ExecuteSqlCommand("CREATE DATABASE " + DbName + " COLLATE  Cyrillic_General_CI_AS");
           // context.Database.Create();    
            
        //    //Наполним
        //    //Seed(context);
        //}
        
        //protected override void Seed(BadroadsDataContext context)
        //{
            //base.Seed(context);
            //Установим сортировку
            //string DbName = context.Database.Connection.Database;
            //context.Database.UseTransaction(new DbContextTransaction());
            //context.Database.ExecuteSqlCommand("Alter database [" + DbName + "] SET SINGLE_USER");
            //context.Database.ExecuteSqlCommand("Alter database [" + DbName + "] collate Cyrillic_General_CI_AS");
            //context.Database.ExecuteSqlCommand("ALTER TABLE [Defects] ALTER COLUMN NAME VARCHAR(50) COLLATE  Cyrillic_General_CI_AS");
            
            //Дефолтные виды проблем
            //context.Database.ExecuteSqlCommand("insert into Defects(Name) values('Яма')");
            //context.Database.ExecuteSqlCommand("insert into Defects(Name) values('Открытый люк')");
            //context.Database.ExecuteSqlCommand("insert into Defects(Name) values('Отсутствие разметки')");

            // заглушка. чтобы наполнить список с точками, которых пока нет в базе
            //Defect d = context.Defects.First();
            //for (int x = 0; x < 100; x++)
            //{
            //    double latitude = 48.459015 + (x * 0.00045);
            //    double longitude = 35.042302 + (x * 0.00045);
            //    string adress = String.Format("Проблема на улице " + x);

            //    GeoData g = new GeoData(latitude, longitude, adress);

            //    Point p = new Point();
            //    p.GeoData = g;
            //    p.Defect = d;

            //    context.Points.Add(p);
            //}
            
        }

    }

    /// <summary>Контекст подключения к БД</summary>
    public class BadroadsDataContext : DbContext
    {
        /// <summary>Объект подключения к БД</summary>
        /// <param name="nameOrConnectionString">Строка подключения или имя строки подключения из КОНФИГ файла</param>
        public BadroadsDataContext(string nameOrConnectionString = "DefaultConnection")
            : base(nameOrConnectionString)
        {
            //Database.SetInitializer(new DropCreateDatabaseAlways<BadroadsDataContext>());
            Database.SetInitializer(new DbInitializer());
        }

        //Построитель модели
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            //Связь Поинта и Коментариев
            modelBuilder.Entity<Point>()
                .HasMany(p => p.Comments)
                .WithMany(c => c.Points)
                .Map(mc =>
                {
                    mc.ToTable("PointsJoinComents");
                    //mc.MapLeftKey("id_Point");
                    //mc.MapRightKey("id_Comment");
                });

            //Связь Поинта и Фото
            modelBuilder.Entity<Point>()
                .HasMany(po => po.Photos)
                .WithMany(ph => ph.Points)
                .Map(mc =>
                {
                    mc.ToTable("PointsJoinPhotos");
                    //mc.MapLeftKey("id_Point");
                    //mc.MapRightKey("id_Photo");
                });
            
        }

        public DbSet<Point> Points { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Defect> Defects { get; set; }
        private DbSet<UserProfile> Users { get; set; }

        /// <summary>Получение ссылки на профиль пользователя</summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public UserProfile GetUSerProfile(System.Security.Principal.IPrincipal User)
        {
            return this.Users.First(U => U.UserName == User.Identity.Name);
        }
    }

    /// <summary>Базовый клас для всех класов БД</summary>
    public class  BadroadsDataItem
    {
        [Key]
        public int ID { get; private set; }
    }

    /// <summary>Точка дефекта на дороге</summary>
    public class Point : BadroadsDataItem
    {
        public Point()
        {
            //this.Autor = UProfile;
            this.Date = DateTime.Now;
            this.isValid = false;
        }
        
        /// <summary>Дата и Время публикации дефекта</summary>
        [Required]
        public DateTime Date { get; private set; }

        /// <summary>Разновидность дефекта</summary>
        [Required]
        public virtual Defect Defect { get; set; }
        
        /// <summary>Рейтинг ямы</summary>
        public int Rate { get; set; }

        /// <summary>Статус проверена или нет</summary>
        public bool isValid { get; set; }

        /// <summary>Автор</summary>
        //[Required]
        public virtual UserProfile Autor { get; set; }

         /// <summary>Метаданные гугл мама, координаты точки</summary>
        public virtual GeoData GeoData { get; set; }

        /// <summary>Обложка дефекта</summary>
        public virtual Photo Cover { get; set; } 

        /// <summary>Коллекция комментариев</summary>
        public virtual ICollection<Comment> Comments { get; set; }

        /// <summary>Коллекция фотографий</summary>
        public virtual ICollection<Photo> Photos { get; set; }

        public void AddPhoto(Photo p)
        {
            this.Photos.Add(p);
        }

        public void AddComent(Comment C)
        {
            this.Comments.Add(C);
        }
    }

    /// <summary>Гео данные  ГуглМапс</summary>
    public class GeoData : BadroadsDataItem
    {
        public GeoData(double latitude, double longitude, string fullAddress="")
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.FullAddress = fullAddress;
        }

        public GeoData()
        { }

        [Required]
        public double Latitude{get;private set;}
        [Required]
        public double Longitude{get;private set;}
        
        /// <summary>Точный адрес объекта</summary>
        public string FullAddress{get;set;}
    }

    /// <summary>Дефект дороги</summary>
    public class Defect: BadroadsDataItem
    {
        [Required,MaxLength(50)]
        public string Name { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>Фотография дефекта на дороге</summary>
    public class Photo : BadroadsDataItem
    {
        [Required,Url]
        public string Url { get; set; }

        public virtual ICollection<Point> Points { get; set; }

    }

    /// <summary>Комментарий к Дефекту</summary>
    public class Comment : BadroadsDataItem
    {
        public Comment()
        {
          // this.Autor = UProfile;
            this.Date = DateTime.Now; 
        }
        
        /// <summary>Дата и Время публикации комментария</summary>
        [Required]
        public DateTime Date { get; private set; }

        /// <summary>Сам текст комментария</summary>
        [Required]
        [Display(Name = "Comment")]
        public string ContentText { get; set; }

        /// <summary>Автор Комментария</summary>
        //[Required]
        public virtual UserProfile Autor { get;  set; }

        public virtual ICollection<Point> Points { get; set; }
          
    }


}
