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
    class DbInitializer : DropCreateDatabaseIfModelChanges<BadroadsDataContext>
    {
        protected override void Seed(BadroadsDataContext context)
        {
            base.Seed(context);

            //Дефолтные виды проблем
            context.Defects.Add(new Defect() { Name = "Яма"});
            context.Defects.Add(new Defect() { Name = "Открытый люк"});
            context.Defects.Add(new Defect() { Name = "Отсутствие разметки"});
            context.Defects.Add(new Defect() { Name = "Забитая ливневка" });
            context.SaveChanges();
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
        public DbSet<UserProfile> Users { get; set; }

        /// <summary>Получение ссылки на профиль пользователя</summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public UserProfile GetUSerProfile(System.Security.Principal.IPrincipal User)
        {
            return this.Users.First(U => U.UserName == User.Identity.Name);
        }

        /// <summary>
        /// Получение ссылки на дефект
        /// </summary>
        /// <param name="DefectName">Имя Дефекта</param>
        /// <returns></returns>
        public Defect GetDefect(string DefectName)
        {
            return this.Defects.First(d => d.Name == DefectName);
        }

        /// <summary>
        /// Получение ссылки на дефект
        /// </summary>
        /// <param name="DefectId">ИД Дефекта</param>
        /// <returns></returns>
        public Defect GetDefect(int DefectId)
        {
            return this.Defects.First(d => d.ID == DefectId);
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

            this.Photos = new HashSet<Photo>();
            this.Comments = new HashSet<Comment>();
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
        public DateTime GetLastCommentDate()
        {
            DateTime lastCommentDate = new DateTime(1970, 01, 01, 00, 00, 00);//для инициализации  переменной используем дату 01.01.1970
            foreach (var item in this.Comments)
            {
                if (item.Date > lastCommentDate)
                    lastCommentDate = item.Date;
            }
            return lastCommentDate;
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
        [Required]
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
