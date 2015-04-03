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
    class DbInitializer : CreateDatabaseIfNotExists<BadroadsDataContext>
    {
        protected override void Seed(BadroadsDataContext context)
        {
            base.Seed(context);
            context.Database.ExecuteSqlCommand("insert into Defects(Name) values('Яма')");
            context.Database.ExecuteSqlCommand("insert into Defects(Name) values('Открытый люк')");
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
        public DbSet<Photo> Photos { get; private set; }
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
        public Point(UserProfile UProfile)
        {
            this.Autor = UProfile;
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
        public virtual UserProfile Autor { get; private set; }

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
        public GeoData(double latitude,double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

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
        public Comment(UserProfile UProfile)
        {
            this.Autor = UProfile;
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
        public virtual UserProfile Autor { get; private set; }

        public virtual ICollection<Point> Points { get; set; }
          
    }


}
