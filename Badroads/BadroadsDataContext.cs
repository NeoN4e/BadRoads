using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace Badroads
{
    /// <summary>Контекст подключения к БД</summary>
    public class BadroadsDataContext : DbContext
    {
        /// <summary>Обїект подключения к БД</summary>
        /// <param name="nameOrConnectionString">Строка подключения или имя строки подключения из КОНФИГ файла</param>
        public BadroadsDataContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {}

        public DbSet<Point> Points { get; set; }
        public DbSet<Photo> Photos { get; private set; }
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
        { this.Date = DateTime.Now; }

        /// <summary>Метаданные гугл мама, координаты точки</summary>
        public string GooleMapInfo { get; set; }

        /// <summary>Дата и Время публикации дефекта</summary>
        public DateTime Date { get; private set; }

        /// <summary>Рейтинг ямы</summary>
        public int Rate { get; set; }

        /// <summary>Статус проверена или нет</summary>
        public bool isValid { get; set; }

        /// <summary>Разновидность дефекта</summary>
        public virtual Defect Defect { get; set; }

        /// <summary>Коллекция комментариев</summary>
        public virtual ICollection<Comment> Coments { get; set; }

        /// <summary>Коллекция фотографий</summary>
        public virtual ICollection<Photo> Photos { get; set; }
    }

    /// <summary>Дефект дороги</summary>
    public class Defect: BadroadsDataItem
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>Фотография дефекта на дороге</summary>
    public class Photo : BadroadsDataItem
    {
        [Url]
        public string Url { get; set; }

        public virtual ICollection<Point> Points { get; set; }

        /// <summary>Получение родителя текущей картинки</summary>
        public Point GetPoint()
        {
            return Points.First();
        }
    }

    /// <summary>Комментарий к Дефекту</summary>
    public class Comment : BadroadsDataItem
    {
        public Comment()
        { this.Date = DateTime.Now; }

        /// <summary>Дата и Время публикации комментария</summary>
        public DateTime Date { get; private set; }

        /// <summary>Сам текст комментария</summary>
        public string ContentText { get; set; }

        public virtual ICollection<Point> Points { get; set; }

        /// <summary>Получение родителя текущего комметнария</summary>
        public Point GetPoint()
        {
            return Points.First();
        }
    }


}
