﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace BadRoads.Models
{
    /// <summary>Контекст подключения к БД</summary>
    public class BadroadsDataContext : DbContext
    {
        /// <summary>Объект подключения к БД</summary>
        /// <param name="nameOrConnectionString">Строка подключения или имя строки подключения из КОНФИГ файла</param>
        public BadroadsDataContext(string nameOrConnectionString = "DefaultConnection")
            : base(nameOrConnectionString)
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<BadroadsDataContext>());
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
        
        /// <summary>Автор</summary>
        [Required]
        public UserProfile Autor { get; private set; }

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

        /// <summary>Автор Комментария</summary>
        [Required] 
        public UserProfile Autor { get; private set; }
 
        /// <summary>Дата и Время публикации комментария</summary>
        [Required]
        public DateTime Date { get; private set; }

        /// <summary>Сам текст комментария</summary>
        [Required]
        [Display(Name = "Comment")]
        public string ContentText { get; set; }

        public virtual ICollection<Point> Points { get; set; }
          
    }


}
